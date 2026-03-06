using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SquadPlaces.Data;

/// <summary>
/// Factory for configuring storage services based on environment settings.
/// 
/// Configuration:
/// - STORAGE_MODE=Blob (default): Uses Azure Blob Storage (requires BlobStorage connection)
/// - STORAGE_MODE=File: Uses file-based storage (requires FILE_STORAGE_PATH)
/// 
/// For Docker deployments with volume mounts, set:
///   STORAGE_MODE=File
///   FILE_STORAGE_PATH=/data
/// </summary>
public static class StorageServiceFactory
{
    /// <summary>
    /// Registers the appropriate IBlobStorageService based on STORAGE_MODE configuration.
    /// </summary>
    public static IServiceCollection AddStorageService(
        this IServiceCollection services, 
        IConfiguration configuration,
        ILoggerFactory? loggerFactory = null)
    {
        var storageMode = configuration["STORAGE_MODE"] ?? "Blob";
        var logger = loggerFactory?.CreateLogger("StorageServiceFactory");

        if (storageMode.Equals("File", StringComparison.OrdinalIgnoreCase))
        {
            var basePath = configuration["FILE_STORAGE_PATH"] ?? "/data";
            logger?.LogInformation("Configuring FileStorageService with base path: {BasePath}", basePath);
            
            services.AddSingleton<IBlobStorageService>(_ =>
            {
                var service = new FileStorageService(basePath);
                service.InitializeAsync().GetAwaiter().GetResult();
                return service;
            });
        }
        else
        {
            logger?.LogInformation("Configuring BlobStorageService (Azure Blob Storage)");
            services.AddSingleton<IBlobStorageService, BlobStorageService>();
        }

        return services;
    }

    /// <summary>
    /// Returns true if the current configuration uses file-based storage.
    /// </summary>
    public static bool IsFileStorage(IConfiguration configuration)
    {
        var mode = configuration["STORAGE_MODE"] ?? "Blob";
        return mode.Equals("File", StringComparison.OrdinalIgnoreCase);
    }
}
