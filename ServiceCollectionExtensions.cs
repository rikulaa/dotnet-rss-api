namespace Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiValidation(
        this IServiceCollection services)
    {
        return services.AddValidation();
    }
}