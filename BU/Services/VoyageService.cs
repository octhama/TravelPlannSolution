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
                        // Créer un nouveau voyage avec UNIQUEMENT les propriétés de base
                        var newVoyage = new Voyage
                        {
                            NomVoyage = voyage.NomVoyage,
                            Description = voyage.Description,
                            DateDebut = voyage.DateDebut,
                            DateFin = voyage.DateFin,
                            EstComplete = voyage.EstComplete,
                            EstArchive = voyage.EstArchive
                        };

                        // Ajouter le voyage SANS les relations many-to-many
                        await _context.Voyages.AddAsync(newVoyage);
                        await _context.SaveChangesAsync();

                        Debug.WriteLine($"Voyage créé avec ID: {newVoyage.VoyageId}");

                        // Maintenant gérer les relations many-to-many séparément
                        if (voyage.Activites != null && voyage.Activites.Any())
                        {
                            var activiteIds = voyage.Activites.Select(a => a.ActiviteId).ToList();
                            var existingActivites = await _context.Activites
                                .Where(a => activiteIds.Contains(a.ActiviteId))
                                .ToListAsync();
                            
                            Debug.WriteLine($"Trouvé {existingActivites.Count} activités existantes");
                            
                            // Recharger le voyage pour établir les relations
                            var voyageForRelations = await _context.Voyages
                                .Include(v => v.Activites)
                                .FirstAsync(v => v.VoyageId == newVoyage.VoyageId);
                            
                            foreach (var activite in existingActivites)
                            {
                                voyageForRelations.Activites.Add(activite);
                            }
                        }

                        if (voyage.Hebergements != null && voyage.Hebergements.Any())
                        {
                            var hebergementIds = voyage.Hebergements.Select(h => h.HebergementId).ToList();
                            var existingHebergements = await _context.Hebergements
                                .Where(h => hebergementIds.Contains(h.HebergementId))
                                .ToListAsync();
                            
                            Debug.WriteLine($"Trouvé {existingHebergements.Count} hébergements existants");
                            
                            // Recharger le voyage pour établir les relations (si pas déjà fait)
                            var voyageForRelations = await _context.Voyages
                                .Include(v => v.Hebergements)
                                .FirstAsync(v => v.VoyageId == newVoyage.VoyageId);
                            
                            foreach (var hebergement in existingHebergements)
                            {
                                voyageForRelations.Hebergements.Add(hebergement);
                            }
                        }

                        // Sauvegarder les relations
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        
                        Debug.WriteLine("Voyage et relations sauvegardés avec succès");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Erreur dans la transaction: {ex}");
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

                        // Vider les collections existantes
                        existingVoyage.Activites.Clear();
                        existingVoyage.Hebergements.Clear();
                        
                        // Sauvegarder pour supprimer les relations existantes
                        await _context.SaveChangesAsync();

                        // Ajouter les nouvelles relations
                        if (voyage.Activites != null && voyage.Activites.Any())
                        {
                            var activiteIds = voyage.Activites.Select(a => a.ActiviteId).ToList();
                            var activitesFromDb = await _context.Activites
                                .Where(a => activiteIds.Contains(a.ActiviteId))
                                .ToListAsync();

                            foreach (var activite in activitesFromDb)
                            {
                                existingVoyage.Activites.Add(activite);
                            }
                        }

                        if (voyage.Hebergements != null && voyage.Hebergements.Any())
                        {
                            var hebergementIds = voyage.Hebergements.Select(h => h.HebergementId).ToList();
                            var hebergementsFromDb = await _context.Hebergements
                                .Where(h => hebergementIds.Contains(h.HebergementId))
                                .ToListAsync();

                            foreach (var hebergement in hebergementsFromDb)
                            {
                                existingVoyage.Hebergements.Add(hebergement);
                            }
                        }

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
                        // Chercher si l'activité existe déjà
                        var existingActivite = await _context.Activites
                            .FirstOrDefaultAsync(a => a.ActiviteId == activite.ActiviteId);

                        // Si l'activité n'existe pas, la créer
                        if (existingActivite == null)
                        {
                            var newActivite = new Activite
                            {
                                Nom = activite.Nom,
                                Description = activite.Description
                            };
                            await _context.Activites.AddAsync(newActivite);
                            await _context.SaveChangesAsync();
                            existingActivite = newActivite;
                        }

                        // Charger le voyage avec ses activités
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
                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    
                    try
                    {
                        // Chercher si l'hébergement existe déjà
                        var existingHebergement = await _context.Hebergements
                            .FirstOrDefaultAsync(h => h.HebergementId == hebergement.HebergementId);

                        // Si l'hébergement n'existe pas, le créer
                        if (existingHebergement == null)
                        {
                            var newHebergement = new Hebergement
                            {
                                Nom = hebergement.Nom,
                                TypeHebergement = hebergement.TypeHebergement,
                                Cout = hebergement.Cout
                            };
                            await _context.Hebergements.AddAsync(newHebergement);
                            await _context.SaveChangesAsync();
                            existingHebergement = newHebergement;
                        }

                        // Charger le voyage avec ses hébergements
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
                        // Méthode alternative : charger le voyage avec ses relations
                        // mais détacher immédiatement pour éviter les problèmes de tracking
                        var voyage = await _context.Voyages
                            .Include(v => v.Activites)
                            .Include(v => v.Hebergements)
                            .FirstOrDefaultAsync(v => v.VoyageId == voyageId);

                        if (voyage != null)
                        {
                            // Vider les collections de relations
                            voyage.Activites.Clear();
                            voyage.Hebergements.Clear();
                            
                            // Sauvegarder pour supprimer les relations
                            await _context.SaveChangesAsync();
                            
                            // Détacher l'entité pour éviter les conflits
                            _context.Entry(voyage).State = EntityState.Detached;
                            
                            // Créer une nouvelle instance avec seulement l'ID pour la suppression
                            var voyageToDelete = new Voyage { VoyageId = voyageId };
                            _context.Entry(voyageToDelete).State = EntityState.Deleted;
                            
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