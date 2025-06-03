using BikeHistory.Server.Data;
using BikeHistory.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BikeHistory.Server.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            try
            {
                await context.Database.MigrateAsync();

                // Create roles
                string[] roleNames = { "Admin", "User", "Store" };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                        logger.LogInformation($"Created role: {roleName}");
                    }
                }

                // Create admin user
                var adminEmail = config["AdminUser:Email"];
                if (!string.IsNullOrEmpty(adminEmail))
                {
                    var adminUser = await userManager.FindByEmailAsync(adminEmail);
                    if (adminUser == null)
                    {
                        adminUser = new ApplicationUser
                        {
                            UserName = adminEmail,
                            Email = adminEmail,
                            FirstName = "Admin",
                            LastName = "User",
                            EmailConfirmed = true
                        };

                        var adminPassword = config["AdminUser:Password"];
                        if (!string.IsNullOrEmpty(adminPassword))
                        {
                            var result = await userManager.CreateAsync(adminUser, adminPassword);

                            if (result.Succeeded)
                            {
                                await userManager.AddToRoleAsync(adminUser, "Admin");
                                logger.LogInformation($"Created admin user: {adminEmail}");
                            }
                            else
                            {
                                foreach (var error in result.Errors)
                                {
                                    logger.LogError($"Error creating admin user: {error.Description}");
                                }
                            }
                        }
                        else
                        {
                            logger.LogError("Admin password not configured. Cannot create admin user.");
                        }
                    }
                }
                else
                {
                    logger.LogWarning("Admin email not configured. Skipping admin user creation.");
                }

                // Seed bike types
                if (!context.BikeTypes.Any())
                {
                    var bikeTypes = new List<BikeType>
                    {
                        new BikeType { Name = "Road Bike", Description = "Designed for paved roads" },
                        new BikeType { Name = "Mountain Bike", Description = "Designed for off-road cycling" },
                        new BikeType { Name = "Hybrid Bike", Description = "Combines features of road and mountain bikes" },
                        new BikeType { Name = "City Bike", Description = "Designed for urban environments" },
                        new BikeType { Name = "Folding Bike", Description = "Can be folded for storage or transport" },
                        new BikeType { Name = "Electric Bike", Description = "Features an integrated electric motor" }
                    };

                    context.BikeTypes.AddRange(bikeTypes);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Seeded bike types");
                }

                // Seed manufacturers
                if (!context.Manufacturers.Any())
                {
                    var manufacturers = new List<Manufacturer>
                    {
                        new Manufacturer { Name = "Trek", CountryOfOrigin = "USA", Website = "https://www.trekbikes.com" },
                        new Manufacturer { Name = "Giant", CountryOfOrigin = "Taiwan", Website = "https://www.giant-bicycles.com" },
                        new Manufacturer { Name = "Specialized", CountryOfOrigin = "USA", Website = "https://www.specialized.com" },
                        new Manufacturer { Name = "Cannondale", CountryOfOrigin = "USA", Website = "https://www.cannondale.com" },
                        new Manufacturer { Name = "Scott", CountryOfOrigin = "Switzerland", Website = "https://www.scott-sports.com" }
                    };

                    context.Manufacturers.AddRange(manufacturers);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Seeded manufacturers");
                }

                // Seed brands (some brands are directly from manufacturers)
                if (!context.Brands.Any())
                {
                    // Get manufacturers for reference
                    var trekManufacturer = await context.Manufacturers.FirstOrDefaultAsync(m => m.Name == "Trek");
                    var giantManufacturer = await context.Manufacturers.FirstOrDefaultAsync(m => m.Name == "Giant");
                    var specializedManufacturer = await context.Manufacturers.FirstOrDefaultAsync(m => m.Name == "Specialized");
                    var cannondaleManufacturer = await context.Manufacturers.FirstOrDefaultAsync(m => m.Name == "Cannondale");
                    
                    var brands = new List<Brand>
                    {
                        new Brand { Name = "Trek", ManufacturerId = trekManufacturer?.Id },
                        new Brand { Name = "Bontrager", ManufacturerId = trekManufacturer?.Id, Description = "Trek's in-house component brand" },
                        new Brand { Name = "Giant", ManufacturerId = giantManufacturer?.Id },
                        new Brand { Name = "Liv", ManufacturerId = giantManufacturer?.Id, Description = "Giant's women's specific brand" },
                        new Brand { Name = "Specialized", ManufacturerId = specializedManufacturer?.Id },
                        new Brand { Name = "Cannondale", ManufacturerId = cannondaleManufacturer?.Id },
                        new Brand { Name = "Schwinn", Description = "Classic American bicycle brand" },
                        new Brand { Name = "Bianchi", Description = "Italian bicycle manufacturer" }
                    };

                    context.Brands.AddRange(brands);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Seeded brands");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }
    }
}