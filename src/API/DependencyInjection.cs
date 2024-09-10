using Azure.Identity;
using NSwag;
using NSwag.Generation.Processors.Security;
using API.Infrastructure;
using API.Services;
using Application.Common.Interfaces;
using Infrastructure.Data;
using ZymLabs.NSwag.FluentValidation;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
  public static IServiceCollection AddWebServerServices(this IServiceCollection services)
  {

    services.AddDatabaseDeveloperPageExceptionFilter();

    services.AddScoped<IUser, CurrentUser>();

    services.AddHttpContextAccessor();

    services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>();

    services.AddExceptionHandler<CustomExceptionHandler>();

    services.AddScoped(provider =>
            {
              var validationRules = provider.GetService<IEnumerable<FluentValidationRule>>();
              var loggerFactory = provider.GetService<ILoggerFactory>();

              return new FluentValidationSchemaProcessor(provider, validationRules, loggerFactory);
            });

    services.Configure<ApiBehaviorOptions>(options =>
        options.SuppressModelStateInvalidFilter = true);

    services.AddEndpointsApiExplorer();

    services.AddOpenApiDocument((configure, sp) =>
    {
      configure.Title = "CleanArchitecture API";

      var fluentValidationSchemaProcessor =
              sp.CreateScope().ServiceProvider.GetRequiredService<FluentValidationSchemaProcessor>();

      configure.SchemaSettings.SchemaProcessors.Add(fluentValidationSchemaProcessor);

      configure.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
      {
        Type = OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = OpenApiSecurityApiKeyLocation.Header,
        Description = "Type into the textbox: Bearer {your JWT token}."
      });

      configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
    });

    return services;
  }

  public static IServiceCollection AddKeyVaultIfConfigured(this IServiceCollection services, ConfigurationManager configuration)
  {
    var keyVaultUri = configuration["KeyVaultUri"];
    if (!string.IsNullOrWhiteSpace(keyVaultUri))
    {
      configuration.AddAzureKeyVault(
          new Uri(keyVaultUri),
          new DefaultAzureCredential());
    }

    return services;
  }
}