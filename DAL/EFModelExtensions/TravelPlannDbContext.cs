using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DAL.DB;
using Common.Utilities;

namespace DAL.EFModelExtensions
{
    public partial class TravelPlannDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Optionnel : Afficher le chemin courant pour débugger
                var currentDir = Directory.GetCurrentDirectory();
                System.Console.WriteLine("Current Directory: " + currentDir);
                
                var config = new ConfigurationBuilder()
                    .SetBasePath(currentDir)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();

                var connectionString = AppSettingsHelper.GetConnectionString("TravelPlannConnectionString");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("La chaîne de connexion n'est pas initialisée. Vérifiez que le fichier appsettings.json est présent et correctement configuré.");
                }

                optionsBuilder
                    .UseLazyLoadingProxies()
                    .UseSqlServer(connectionString);
            }
        }

        public virtual DbSet<Voyage> Voyage { get; set; }
    }
}