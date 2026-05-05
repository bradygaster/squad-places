using Microsoft.Extensions.DependencyInjection;
using SquadPlaces.Api.Endpoints.Services;

namespace SquadPlaces.Api.Endpoints;

/// <summary>
/// Registers API-layer services: IP blocklist, duplicate detection, comment duplicate detection, image generation.
/// Does NOT register storage — that is a host responsibility.
/// </summary>
public static class ApiServiceRegistration
{
    public static IServiceCollection AddSquadPlacesApiServices(this IServiceCollection services)
    {
        services.AddSingleton<IpBlocklistService>();
        services.AddSingleton<DuplicateDetectionService>();
        services.AddSingleton<CommentDuplicateDetectionService>();
        services.AddSingleton<IImageGenerationService, NanoBananaImageGenerationService>();
        
        return services;
    }
}
