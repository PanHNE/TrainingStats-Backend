using Microsoft.AspNetCore.Identity;
using TrainingStats.Infrastructure.Identity;

namespace TrainingStats.Infrastructure.Data.Seeders;

public class UserSeeder
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserSeeder(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ApplicationUser> Seed(IdentityRole identityRole)
    {

        var adminUserName = Environment.GetEnvironmentVariable("TS_ADMIN_USERNAME");
        var adminEmail = Environment.GetEnvironmentVariable("TS_ADMIN_EMAIL");
        var adminPassword = Environment.GetEnvironmentVariable("TS_ADMIN_PASSWORD");

        Guard.Against.Null(adminUserName, message: "[UserSeeder] Admin user name not found.");
        Guard.Against.Null(adminEmail, message: "[UserSeeder] Admin email not found.");
        Guard.Against.Null(adminPassword, message: "[UserSeeder] Admin password not found.");

        var administrator = new ApplicationUser { UserName = adminUserName, Email = adminEmail };

        if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            await _userManager.CreateAsync(administrator, adminPassword);
            if (!string.IsNullOrWhiteSpace(identityRole.Name))
            {
                await _userManager.AddToRolesAsync(administrator, new[] { identityRole.Name });
            }
        }

        return administrator;
    }
}
