using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using DAL.DB;

namespace TravelPlannConsoleAppTest;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Initialisation de l'application...");

        // Configuration pour la console
        var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();

        var options = new DbContextOptionsBuilder<TravelPlannDbContext>()
            .UseSqlServer(config.GetConnectionString("TravelPlannConnectionString"))
            .Options;

        using var context = new TravelPlannDbContext(options);
        var voyageService = new BU.Services.VoyageService(context);

        Console.WriteLine("\nListe des Voyages :");
        Console.WriteLine("-------------------");
        
        try
        {
            var voyages = voyageService.GetVoyagesAsync().Result;
            
            if (voyages.Any())
            {
                foreach (var voyage in voyages)
                {
                    Console.WriteLine($"- {voyage.NomVoyage}");
                    Console.WriteLine($"  Description: {voyage.Description}");
                    Console.WriteLine($"  Dates: {voyage.DateDebut:dd/MM/yyyy} au {voyage.DateFin:dd/MM/yyyy}");
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("Aucun voyage trouvé dans la base de données.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nERREUR: {ex.Message}");
            Console.WriteLine("Détails techniques:");
            Console.WriteLine(ex.ToString());
        }

        Console.WriteLine("\nAppuyez sur une touche pour quitter...");
        Console.ReadKey();
    }
}