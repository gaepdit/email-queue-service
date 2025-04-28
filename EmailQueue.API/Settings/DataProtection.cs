using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EmailQueue.API.Settings;

public static class DataProtectionExtensions
{
    // Add a DbContext to store Data Protection Keys.
    public static async Task AddDataProtectionServices(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DataProtectionKeys");

        await new DataProtectionKeysContext(new DbContextOptionsBuilder<DataProtectionKeysContext>()
            .UseSqlite(connectionString).Options).Database.EnsureCreatedAsync();

        builder.Services.AddDbContext<DataProtectionKeysContext>(options => options.UseSqlite(connectionString));
        builder.Services.AddDataProtection().PersistKeysToDbContext<DataProtectionKeysContext>();
    }
}

internal class DataProtectionKeysContext(DbContextOptions<DataProtectionKeysContext> options)
    : DbContext(options), IDataProtectionKeyContext
{
    // This maps to the table that stores keys.
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
}
