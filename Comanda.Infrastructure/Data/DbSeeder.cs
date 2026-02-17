using Comanda.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Comanda.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ComandaDbContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.Users.AnyAsync())
            return;

        var users = new[]
        {
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Mozo Demo",
                Role = UserRole.Waiter
            },
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Cocinero Demo",
                Role = UserRole.Cook
            }
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }
}
