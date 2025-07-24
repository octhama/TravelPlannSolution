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
                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    
                    try
                    {
                        // Créer un nouveau voyage avec les propriétés de base
                        var newVoyage = new Voyage
                        {
                            NomVoyage = voyage.NomVoyage,
                            Description = voyage.Description,
                            DateDebut = voyage.DateDebut,
                            DateFin = voyage.DateFin,
                            EstComplete = voyage.EstComplete,
                            EstArchive = voyage.EstArchive
                        };

                        // Ajouter d'abord le voyage pour obtenir un ID
                        await _context.Voyages.AddAsync(newVoyage);
                        await _context.SaveChangesAsync();

                        // Gestion des activités - récupérer les entités existantes depuis la BD
                        if (voyage.Activites != null && voyage.Activites.Any())
                        {
                            var activiteIds = voyage.Activites.Select(a => a.ActiviteId).ToList();
                            var existingActivites = await _context.Activites
                                .Where(a => activiteIds.Contains(a.ActiviteId))
                                .ToListAsync();
                            
                            // Attacher les activités existantes au voyage
                            foreach (var activite in existingActivites)
                            {
                                newVoyage.Activites.Add(activite);
                            }
                        }

                        // Gestion des hébergements - récupérer les entités existantes depuis la BD
                        if (voyage.Hebergements != null && voyage.Hebergements.Any())
                        {
                            var hebergementIds = voyage.Hebergements.Select(h => h.HebergementId).ToList();
                            var existingHebergements = await _context.Hebergements
                                .Where(h => hebergementIds.Contains(h.HebergementId))
                                .ToListAsync();
                            
                            // Attacher les hébergements existants au voyage
                            foreach (var hebergement in existingHebergements)
                            {
                                newVoyage.Hebergements.Add(hebergement);
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

                        // Gestion des relations many-to-many avec la nouvelle approche
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
                
                _cachedVoyages = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de la mise à jour du voyage: {ex}");
                throw;
            }
        }

        // Méthode helper corrigée pour gérer les activités
        private async Task UpdateVoyageActivitesAsync(Voyage existingVoyage, ICollection<Activite> newActivites)
        {
            if (newActivites == null) newActivites = new List<Activite>();

            // Vider complètement la collection existante
            existingVoyage.Activites.Clear();
            
            // Sauvegarder pour supprimer les relations existantes
            await _context.SaveChangesAsync();

            // Ajouter les nouvelles activités
            if (newActivites.Any())
            {
                var activiteIds = newActivites.Select(a => a.ActiviteId).ToList();
                var activitesFromDb = await _context.Activites
                    .Where(a => activiteIds.Contains(a.ActiviteId))
                    .ToListAsync();

                foreach (var activite in activitesFromDb)
                {
                    existingVoyage.Activites.Add(activite);
                }
            }
        }

        // Méthode helper corrigée pour gérer les hébergements
        private async Task UpdateVoyageHebergementsAsync(Voyage existingVoyage, ICollection<Hebergement> newHebergements)
        {
            if (newHebergements == null) newHebergements = new List<Hebergement>();

            // Vider complètement la collection existante
            existingVoyage.Hebergements.Clear();
            
            // Sauvegarder pour supprimer les relations existantes
            await _context.SaveChangesAsync();

            // Ajouter les nouveaux hébergements
            if (newHebergements.Any())
            {
                var hebergementIds = newHebergements.Select(h => h.HebergementId).ToList();
                var hebergementsFromDb = await _context.Hebergements
                    .Where(h => hebergementIds.Contains(h.HebergementId))
                    .ToListAsync();

                foreach (var hebergement in hebergementsFromDb)
                {
                    existingVoyage.Hebergements.Add(hebergement);
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
                            existingActivite = new Activite
                            {
                                Nom = activite.Nom,
                                Description = activite.Description
                            };
                            await _context.Activites.AddAsync(existingActivite);
                            await _context.SaveChangesAsync();
                        }

                        // 2. Lier au voyage
                        var voyage = await _context.Voyages
                            .Include(v => v.Activites)
                            .FirstOrDefaultAsync(v => v.VoyageId == voyageId);
                        
                        if (voyage != null && !voyage.Activites.Any(a => a.ActiviteId == existingActivite.ActiviteId))
                        {
                            // Récupérer l'activité depuis la BD pour s'assurer qu'elle est trackée
                            var activiteFromDb = await _context.Activites.FindAsync(existingActivite.ActiviteId);
                            if (activiteFromDb != null)
                            {
                                voyage.Activites.Add(activiteFromDb);
                                await _context.SaveChangesAsync();
                            }
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
                            existingHebergement = new Hebergement
                            {
                                Nom = hebergement.Nom,
                                TypeHebergement = hebergement.TypeHebergement,
                                Cout = hebergement.Cout
                            };
                            await _context.Hebergements.AddAsync(existingHebergement);
                            await _context.SaveChangesAsync();
                        }

                        // 2. Lier au voyage
                        var voyage = await _context.Voyages
                            .Include(v => v.Hebergements)
                            .FirstOrDefaultAsync(v => v.VoyageId == voyageId);
                        
                        if (voyage != null && !voyage.Hebergements.Any(h => h.HebergementId == existingHebergement.HebergementId))
                        {
                            // Récupérer l'hébergement depuis la BD pour s'assurer qu'il est tracké
                            var hebergementFromDb = await _context.Hebergements.FindAsync(existingHebergement.HebergementId);
                            if (hebergementFromDb != null)
                            {
                                voyage.Hebergements.Add(hebergementFromDb);
                                await _context.SaveChangesAsync();
                            }
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
                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    
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
                            }
                        }

                        await transaction.CommitAsync();
                        _cachedVoyages = null;
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });
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
                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    
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
                            }
                        }

                        await transaction.CommitAsync();
                        _cachedVoyages = null;
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });
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