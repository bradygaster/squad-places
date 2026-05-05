using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace SquadPlaces.Api.Endpoints.Services;

public interface IImageGenerationService
{
    /// <summary>
    /// Calls the nano-banana MCP server to generate an image from a text prompt.
    /// Returns raw PNG bytes, or null when the MCP server is not configured/available.
    /// </summary>
    Task<byte[]?> GenerateImageAsync(string prompt, CancellationToken ct = default);
}

public class NanoBananaImageGenerationService : IImageGenerationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<NanoBananaImageGenerationService> _logger;
    private readonly string _command;
    private readonly string[] _args;
    private readonly string? _googleApiKey;

    public NanoBananaImageGenerationService(
        IConfiguration configuration,
        ILogger<NanoBananaImageGenerationService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        _command = _configuration["NanoBanana:Command"] ?? "npx";
        var argsString = _configuration["NanoBanana:Args"] ?? "-y nano-banana";
        _args = argsString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        _googleApiKey = _configuration["NanoBanana:GoogleApiKey"] 
            ?? Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
    }

    public async Task<byte[]?> GenerateImageAsync(string prompt, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_googleApiKey))
        {
            _logger.LogWarning("Image generation service not configured. Set NanoBanana:GoogleApiKey or GOOGLE_API_KEY.");
            return null;
        }

        try
        {
            var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
            {
                Command = _command,
                Arguments = _args,
                EnvironmentVariables = new Dictionary<string, string?>
                {
                    ["GOOGLE_API_KEY"] = _googleApiKey
                }
            });

            await using var client = await McpClient.CreateAsync(clientTransport, cancellationToken: ct);

            var result = await client.CallToolAsync("generate_image", new Dictionary<string, object?>
            {
                ["prompt"] = prompt
            }, cancellationToken: ct);

            if (result?.Content != null)
            {
                foreach (var content in result.Content)
                {
                    if (content is ImageContentBlock imageBlock)
                    {
                        var decodedData = imageBlock.DecodedData;
                        if (decodedData.Length > 0)
                        {
                            return decodedData.ToArray();
                        }
                    }
                }
            }

            _logger.LogWarning("nano-banana returned no image data");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate image with nano-banana MCP server");
            return null;
        }
    }
}
