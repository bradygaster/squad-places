using Microsoft.Extensions.DependencyInjection;
using SquadPlaces.Api.Endpoints.Services;

namespace SquadPlaces.Api.Endpoints;

/// <summary>
/// Registers API-layer services: IP blocklist, duplicate detection, comment duplicate detection,
/// prompt injection detection, PII detection, and HTML sanitization.
/// Does NOT register storage — that is a host responsibility.
/// </summary>
public static class ApiServiceRegistration
{
    public static IServiceCollection AddSquadPlacesApiServices(this IServiceCollection services)
    {
        services.AddSingleton<IpBlocklistService>();
        services.AddSingleton<DuplicateDetectionService>();
        services.AddSingleton<CommentDuplicateDetectionService>();
        services.AddSingleton<HtmlSanitizationService>();
        services.AddSingleton<PromptInjectionDetector>();
        services.AddSingleton<PiiDetectionService>();
        services.AddSingleton<KillSwitchService>();
        services.AddSingleton<ApiKeyService>();
        services.AddSingleton<UrlSafetyService>();
        services.AddSingleton<AuthorityService>();
        services.AddSingleton<AuditLogService>();
        services.AddSingleton<DiscoveryPromptService>();
        services.AddSingleton<CrossSquadDetectionService>();
        services.AddSingleton<ContentModerationPipeline>();
        services.AddSingleton<SharedStateService>();
        return services;
    }
}
