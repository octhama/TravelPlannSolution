using DAL.DB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using BU.Models;

namespace BU.Services
{
    public class VoyageService : IVoyageService
    {
        private readonly TravelPlannDbContext _context;
        private List<Voyage> _cachedVoyages;
        private DateTime _lastCacheTime;

        public VoyageService(TravelPlannDbContext context)
        {
            _context = context;
        }
        
        public async Task<Voyage> GetVoyageByIdAsync(int voyageId)
        {
            return await _context.Voyages
                .Include(v => v.Activites)
                .Include(v => v.Hebergements)
                .FirstOrDefaultAsync(v => v.VoyageId == voyageId);
        }

        public async Task<List<Voyage>> GetVoyagesAsync()
        {
            return await _context.Voyages
                .Include(v => v.Activites)
                .Include(v => v.Hebergements)
                .ToListAsync();
        }

        public async Task<List<Voyage>> GetVoyagesAsync(bool forceRefresh = false)
        {
            try
            {
                if (!forceRefresh && _cachedVoyages != null &&
                    (DateTime.Now - _lastCacheTime).TotalMinutes < 5)
                {
                    return _cachedVoyages;
                }

                _cachedVoyages = await _context.Voyages
                    .AsNoTracking()
                    .OrderByDescending(v => v.DateDebut)
                    .ToListAsync();

                _lastCacheTime = DateTime.Now;
                return _cachedVoyages;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DB Error: {ex}");
                throw;
            }
        }

        public async Task AddVoyageAsync(Voyage voyage)
        {
            if (voyage == null)
                throw new ArgumentNullException(nameof(voyage));

            try
            {
                // Utiliser la stratégie d'exécution pour gérer les transactions
                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    
                    try
                    {
                        // Crée un nouveau voyage avec les propriétés de base
                        var newVoyage = new Voyage
                        {
                            NomVoyage = voyage.NomVoyage,
                            Description = voyage.Description,
                            DateDebut = voyage.DateDebut,
                            DateFin = voyage.DateFin,
                            EstComplete = voyage.EstComplete,
                            EstArchive = voyage.EstArchive
                        };

                        // Initialise les collections si elles sont null
                        newVoyage.Activites = new List<Activite>();
                        newVoyage.Hebergements = new List<Hebergement>();

                        // Ajoute d'abord le voyage pour obtenir un ID
                        await _context.Voyages.AddAsync(newVoyage);
                        await _context.SaveChangesAsync();

                        // Gestion des activités
                        if (voyage.Activites != null && voyage.Activites.Any())
                        {
                            foreach (var activite in voyage.Activites)
                            {
                                var existingActivite = await _context.Activites.FindAsync(activite.ActiviteId);
                                if (existingActivite != null)
                                {
                                    newVoyage.Activites.Add(existingActivite);
                                }
                            }
                        }

                        // Gestion des hébergements
                        if (voyage.Hebergements != null && voyage.Hebergements.Any())
                        {
                            foreach (var hebergement in voyage.Hebergements)
                            {
                                var existingHebergement = await _context.Hebergements.FindAsync(hebergement.HebergementId);
                                if (existingHebergement != null)
                                {
                                    newVoyage.Hebergements.Add(existingHebergement);
                                }
                            }
                        }

                        // Sauvegarde finale
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });
                
                _cachedVoyages = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERREUR DB: {ex}\n{ex.StackTrace}");
                throw;
            }
        }

        public async Task UpdateVoyageAsync(Voyage voyage)
        {
            try
            {
                // Utiliser la stratégie d'exécution pour gérer les transactions
                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    
                    try
                    {
                        // Charger l'entité existante avec tracking activé
                        var existingVoyage = await _context.Voyages
                            .Include(v => v.Activites)
                            .Include(v => v.Hebergements)
                            .FirstOrDefaultAsync(v => v.VoyageId == voyage.VoyageId);

                        if (existingVoyage == null) 
                        {
                            throw new InvalidOperationException($"Voyage avec ID {voyage.VoyageId} introuvable");
                        }

                        // Mettre à jour les propriétés simples
                        existingVoyage.NomVoyage = voyage.NomVoyage;
                        existingVoyage.Description = voyage.Description;
                        existingVoyage.DateDebut = voyage.DateDebut;
                        existingVoyage.DateFin = voyage.DateFin;
                        existingVoyage.EstComplete = voyage.EstComplete;
                        existingVoyage.EstArchive = voyage.EstArchive;

                        // Gestion des relations many-to-many
                        await UpdateVoyageActivitesAsync(existingVoyage, voyage.Activites);
                        await UpdateVoyageHebergementsAsync(existingVoyage, voyage.Hebergements);

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        
                        Debug.WriteLine($"Voyage {voyage.VoyageId} mis à jour avec succès");
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });
                
                // Invalider le cache
                _cachedVoyages = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de la mise à jour du voyage: {ex}");
                throw;
            }
        }

        // Méthode helper pour gérer les activités
        private async Task UpdateVoyageActivitesAsync(Voyage existingVoyage, ICollection<Activite> newActivites)
        {
            if (newActivites == null) newActivites = new List<Activite>();

            // Supprimer les activités qui ne sont plus dans la liste
            var activitesToRemove = existingVoyage.Activites
                .Where(a => !newActivites.Any(na => na.ActiviteId == a.ActiviteId))
                .ToList();

            foreach (var activite in activitesToRemove)
            {
                existingVoyage.Activites.Remove(activite);
            }

            // Ajouter les nouvelles activités
            foreach (var newActivite in newActivites)
            {
                if (!existingVoyage.Activites.Any(a => a.ActiviteId == newActivite.ActiviteId))
                {
                    var existingActivite = await _context.Activites.FindAsync(newActivite.ActiviteId);
                    if (existingActivite != null)
                    {
                        existingVoyage.Activites.Add(existingActivite);
                    }
                }
            }
        }

        // Méthode helper pour gérer les hébergements
        private async Task UpdateVoyageHebergementsAsync(Voyage existingVoyage, ICollection<Hebergement> newHebergements)
        {
            if (newHebergements == null) newHebergements = new List<Hebergement>();

            // Supprimer les hébergements qui ne sont plus dans la liste
            var hebergementsToRemove = existingVoyage.Hebergements
                .Where(h => !newHebergements.Any(nh => nh.HebergementId == h.HebergementId))
                .ToList();

            foreach (var hebergement in hebergementsToRemove)
            {
                existingVoyage.Hebergements.Remove(hebergement);
            }

            // Ajouter les nouveaux hébergements
            foreach (var newHebergement in newHebergements)
            {
                if (!existingVoyage.Hebergements.Any(h => h.HebergementId == newHebergement.HebergementId))
                {
                    var existingHebergement = await _context.Hebergements.FindAsync(newHebergement.HebergementId);
                    if (existingHebergement != null)
                    {
                        existingVoyage.Hebergements.Add(existingHebergement);
                    }
                }
            }
        }
        
        public async Task<VoyageDetails> GetVoyageDetails(int voyageId)
        {
            return await GetVoyageDetailsAsync(voyageId);
        }

        public async Task<VoyageDetails> GetVoyageDetailsAsync(int voyageId)
        {
            try
            {
                var voyage = await _context.Voyages
                    .AsNoTracking()
                    .Include(v => v.Activites)
                    .Include(v => v.Hebergements)
                    .FirstOrDefaultAsync(v => v.VoyageId == voyageId);

                if (voyage == null) return null;

                return new VoyageDetails
                {
                    Voyage = voyage,
                    Activites = voyage.Activites?.ToList() ?? new List<Activite>(),
                    Hebergements = voyage.Hebergements?.ToList() ?? new List<Hebergement>()
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur dans GetVoyageDetailsAsync: {ex}");
                throw;
            }
        }

        public async Task AddActiviteToVoyageAsync(int voyageId, Activite activite)
        {
            try
            {
                // Utiliser la stratégie d'exécution pour gérer les transactions
                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    
                    try
                    {
                        // 1. Ajouter l'activité si elle n'existe pas
                        var existingActivite = await _context.Activites
                            .FirstOrDefaultAsync(a => a.Nom == activite.Nom && a.Description == activite.Description);
                        
                        if (existingActivite == null)
                        {
                            existingActivite = activite;
                            await _context.Activites.AddAsync(existingActivite);
                            await _context.SaveChangesAsync();
                        }

                        // 2. Lier au voyage
                        var voyage = await _context.Voyages
                            .Include(v => v.Activites)
                            .FirstOrDefaultAsync(v => v.VoyageId == voyageId);
                        
                        if (voyage != null && !voyage.Activites.Any(a => a.ActiviteId == existingActivite.ActiviteId))
                        {
                            voyage.Activites.Add(existingActivite);
                            await _context.SaveChangesAsync();
                        }

                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });
                
                _cachedVoyages = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur AddActiviteToVoyageAsync: {ex}");
                throw;
            }
        }

        public async Task AddHebergementToVoyageAsync(int voyageId, Hebergement hebergement)
        {
            try
            {
                // Utiliser la stratégie d'exécution pour gérer les transactions
                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    
                    try
                    {
                        // 1. Ajouter l'hébergement s'il n'existe pas
                        var existingHebergement = await _context.Hebergements
                            .FirstOrDefaultAsync(h => h.Nom == hebergement.Nom && h.TypeHebergement == hebergement.TypeHebergement);
                        
                        if (existingHebergement == null)
                        {
                            existingHebergement = hebergement;
                            await _context.Hebergements.AddAsync(existingHebergement);
                            await _context.SaveChangesAsync();
                        }

                        // 2. Lier au voyage
                        var voyage = await _context.Voyages
                            .Include(v => v.Hebergements)
                            .FirstOrDefaultAsync(v => v.VoyageId == voyageId);
                        
                        if (voyage != null && !voyage.Hebergements.Any(h => h.HebergementId == existingHebergement.HebergementId))
                        {
                            voyage.Hebergements.Add(existingHebergement);
                            await _context.SaveChangesAsync();
                        }

                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });
                
                _cachedVoyages = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur AddHebergementToVoyageAsync: {ex}");
                throw;
            }
        }

        public async Task RemoveActiviteFromVoyageAsync(int voyageId, int activiteId)
        {
            try
            {
                var voyage = await _context.Voyages
                    .Include(v => v.Activites)
                    .FirstOrDefaultAsync(v => v.VoyageId == voyageId);
                
                if (voyage != null)
                {
                    var activite = voyage.Activites.FirstOrDefault(a => a.ActiviteId == activiteId);
                    if (activite != null)
                    {
                        voyage.Activites.Remove(activite);
                        await _context.SaveChangesAsync();
                        _cachedVoyages = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur RemoveActiviteFromVoyageAsync: {ex}");
                throw;
            }
        }

        public async Task RemoveHebergementFromVoyageAsync(int voyageId, int hebergementId)
        {
            try
            {
                var voyage = await _context.Voyages
                    .Include(v => v.Hebergements)
                    .FirstOrDefaultAsync(v => v.VoyageId == voyageId);
                
                if (voyage != null)
                {
                    var hebergement = voyage.Hebergements.FirstOrDefault(h => h.HebergementId == hebergementId);
                    if (hebergement != null)
                    {
                        voyage.Hebergements.Remove(hebergement);
                        await _context.SaveChangesAsync();
                        _cachedVoyages = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur RemoveHebergementFromVoyageAsync: {ex}");
                throw;
            }
        }

        public async Task DeleteVoyageAsync(int voyageId)
        {
            try
            {
                // Utiliser la stratégie d'exécution pour gérer les transactions
                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    
                    try
                    {
                        // 1. Charger le voyage avec ses relations
                        var voyage = await _context.Voyages
                            .Include(v => v.Activites)
                            .Include(v => v.Hebergements)
                            .FirstOrDefaultAsync(v => v.VoyageId == voyageId);

                        if (voyage != null)
                        {
                            // 2. Supprimer les relations many-to-many d'abord
                            voyage.Activites.Clear();
                            voyage.Hebergements.Clear();
                            
                            // 3. Sauvegarder pour supprimer les relations
                            await _context.SaveChangesAsync();
                            
                            // 4. Supprimer le voyage
                            _context.Voyages.Remove(voyage);
                            await _context.SaveChangesAsync();
                            
                            Debug.WriteLine($"Voyage {voyageId} supprimé avec succès");
                        }

                        await transaction.CommitAsync();
                        
                        // Invalider le cache
                        _cachedVoyages = null;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Debug.WriteLine($"Erreur lors de la suppression du voyage: {ex}");
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DB Error deleting voyage: {ex}");
                throw new InvalidOperationException($"Impossible de supprimer le voyage: {ex.Message}", ex);
            }
        }
    }
}