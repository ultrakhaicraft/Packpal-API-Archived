using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Packpal.DAL.Context;

namespace Packpal.DAL
{
    public class PackpalDbContextFactory : IDesignTimeDbContextFactory<PackpalDbContext>
    {
        public PackpalDbContext CreateDbContext(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Packpal"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .Build();

            // Create DbContextOptionsBuilder
            var optionsBuilder = new DbContextOptionsBuilder<PackpalDbContext>();
            
            // Get connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("DefaultConnection string is not configured.");
            }

            // Configure Npgsql
            optionsBuilder.UseNpgsql(connectionString);

            return new PackpalDbContext(optionsBuilder.Options);
        }
    }
}
