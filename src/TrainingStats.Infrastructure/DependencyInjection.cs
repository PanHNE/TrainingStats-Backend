using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TrainingStats.Application.Common.Interface;
using TrainingStats.Application.Common.Interfaces;
using TrainingStats.Domain.Constants;
using TrainingStats.Infrastructure.Data;
using TrainingStats.Infrastructure.Data.Interceptors;
using TrainingStats.Infrastructure.Identity;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
  {
    var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING_TS_DB");

    Guard.Against.Null(connectionString, message: "Connection string to database not found.");

    services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
    services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

    services.AddDbContext<ApplicationDbContext>((sp, options) =>
      {
        options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
        options.UseSqlServer(connectionString);
      });

    services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
    services.AddScoped<ApplicationDbContextInitialiser>();

    services
      .AddIdentityCore<ApplicationUser>()
      .AddRoles<IdentityRole>()
      .AddEntityFrameworkStores<ApplicationDbContext>()
      .AddApiEndpoints();

    services
      .AddAuthentication()
      .AddBearerToken(IdentityConstants.BearerScheme);

    services.AddSingleton(TimeProvider.System);
    services.AddTransient<IIdentityService, IdentityService>();

    services.AddAuthorization(options =>
        options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));

    return services;
  }
}

public static class InitialiserExtensions
{
  public static async Task InitialiseDatabaseAsync(this WebApplication app)
  {
    using var scope = app.Services.CreateScope();

    var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

    await initialiser.InitialiseAsync();

    await initialiser.SeedAsync();
  }
}
