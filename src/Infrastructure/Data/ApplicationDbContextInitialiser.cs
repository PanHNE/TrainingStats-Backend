using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Infrastructure.Data.Seeders;
using Infrastructure.Identity;

namespace Infrastructure.Data;

public class ApplicationDbContextInitialiser
{
  private readonly ILogger<ApplicationDbContextInitialiser> _logger;
  private readonly ApplicationDbContext _dbContext;
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly RoleManager<IdentityRole> _roleManager;

  public ApplicationDbContextInitialiser
  (
    ILogger<ApplicationDbContextInitialiser> logger,
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager
  )
  {
    _logger = logger;
    _dbContext = dbContext;
    _userManager = userManager;
    _roleManager = roleManager;
  }

  public async Task InitialiseAsync()
  {
    try
    {
      await _dbContext.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An error occurred while initialising the database.");
      throw;
    }
  }

  public async Task SeedAsync()
  {
    try
    {
      await TrySeedAsync();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An error occurred while seeding the database.");
      throw;
    }
  }

  public async Task TrySeedAsync()
  {
    // Default roles
    var roleSeeder = new RoleSeeder(_roleManager);
    var administratorRole = await roleSeeder.Seed();

    // Default users
    var userSeeder = new UserSeeder(_userManager);
    await userSeeder.Seed(administratorRole);
  }

}