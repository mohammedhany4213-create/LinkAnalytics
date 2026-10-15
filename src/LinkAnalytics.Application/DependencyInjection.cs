using LinkAnalytics.Application.Interfaces;
using LinkAnalytics.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LinkAnalytics.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<ILinkService, LinkService>();
        services.AddScoped<IClickTrackingService, ClickTrackingService>();
        services.AddScoped<ILinkAnalyticsService, LinkAnalyticsService>();

        return services;
    }
}
