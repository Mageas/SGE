using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SGE.Core.Entities;

namespace SGE.Infrastructure.Data;

/// <summary>
///     Service responsible for seeding initial data in the database.
///     Creates default roles and admin user if they don't exist.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    ///     Initializes the database with default roles, admin user and manager user.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // 1. Créer les rôles par défaut
        string[] roleNames = { "Admin", "Manager", "User" };
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 2. Créer un utilisateur ADMIN par défaut
        var adminEmail = "admin@sge.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "System",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, "Admin123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        // 3. Créer un utilisateur MANAGER par défaut (AJOUTÉ)
        var managerEmail = "manager@sge.com";
        var managerUser = await userManager.FindByEmailAsync(managerEmail);

        if (managerUser == null)
        {
            var manager = new ApplicationUser
            {
                UserName = "manager",
                Email = managerEmail,
                FirstName = "Manager",
                LastName = "System",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Création avec le mot de passe par défaut
            var result = await userManager.CreateAsync(manager, "Manager123!");

            if (result.Succeeded)
            {
                // Assignation du rôle Manager
                await userManager.AddToRoleAsync(manager, "Manager");
            }
        }
    }
}