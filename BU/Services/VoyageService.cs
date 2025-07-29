using DAL.DB;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BU.Entities;

﻿namespace BU.Services
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

        public async Task<List<Voyage>> GetVoyagesByUtilisateurAsync(int utilisateurId)
        {
            try
            {
                Debug.WriteLine($"Recherche des voyages pour utilisateur ID: {utilisateurId}");

                var voyages = await _context.Voyages
                    .Include(v => v.Activites)
                    .Include(v => v.Hebergements)
                    .Where(v => v.UtilisateurId == utilisateurId)
                    .OrderByDescending(v => v.VoyageId)
                    .ToListAsync();

                Debug.WriteLine($"Requête exécutée, {voyages.Count} voyages trouvés");

                foreach (var voyage in voyages)
                {
                    Debug.WriteLine($"Voyage trouvé: ID={voyage.VoyageId}, Nom={voyage.NomVoyage}, " +
                                $"UtilisateurId={voyage.UtilisateurId}, Complete={voyage.EstComplete}, Archive={voyage.EstArchive}");
                }

                return voyages;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur dans GetVoyagesByUtilisateurAsync: {ex}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
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
                        Debug.WriteLine($"=== DÉBUT AddVoyageAsync ===");
                        Debug.WriteLine($"Voyage: {voyage.NomVoyage}");
                        Debug.WriteLine($"Activités à associer: {voyage.Activites?.Count ?? 0}");
                        Debug.WriteLine($"Hébergements à associer: {voyage.Hebergements?.Count ?? 0}");

                        // 1. Créer le voyage de base SANS les relations many-to-many
                        var newVoyage = new Voyage
                        {
                            NomVoyage = voyage.NomVoyage,
                            Description = voyage.Description,
                            DateDebut = voyage.DateDebut,
                            DateFin = voyage.DateFin,
                            EstComplete = voyage.EstComplete,
                            EstArchive = voyage.EstArchive,
                            UtilisateurId = voyage.UtilisateurId // IMPORTANT: Ne pas oublier l'utilisateur
                        };

                        await _context.Voyages.AddAsync(newVoyage);
                        await _context.SaveChangesAsync();

                        Debug.WriteLine($"Voyage créé avec ID: {newVoyage.VoyageId}");

                        // 2. Gérer les relations avec les activités
                        if (voyage.Activites != null && voyage.Activites.Any())
                        {
                            Debug.WriteLine("=== Gestion des activités ===");

                            // Récupérer les IDs des activités à associer
                            var activiteIds = voyage.Activites.Select(a => a.ActiviteId).ToList();
                            Debug.WriteLine($"IDs des activités: {string.Join(", ", activiteIds)}");

                            // Vérifier que toutes les activités existent dans la base
                            var existingActivites = await _context.Activites
                                .Where(a => activiteIds.Contains(a.ActiviteId))
                                .ToListAsync();

                            Debug.WriteLine($"Activités trouvées en DB: {existingActivites.Count}");

                            if (existingActivites.Count != activiteIds.Count)
                            {
                                var foundIds = existingActivites.Select(a => a.ActiviteId).ToList();
                                var missingIds = activiteIds.Except(foundIds).ToList();
                                throw new InvalidOperationException($"Activités non trouvées avec les IDs: {string.Join(", ", missingIds)}");
                            }

                            // Charger le voyage avec sa collection d'activités pour établir les relations
                            var voyageWithActivites = await _context.Voyages
                                .Include(v => v.Activites)
                                .FirstAsync(v => v.VoyageId == newVoyage.VoyageId);

                            // Associer les activités existantes au voyage
                            foreach (var activite in existingActivites)
                            {
                                voyageWithActivites.Activites.Add(activite);
                                Debug.WriteLine($"Activité associée: {activite.Nom} (ID: {activite.ActiviteId})");
                            }
                        }

                        // 3. Gérer les relations avec les hébergements
                        if (voyage.Hebergements != null && voyage.Hebergements.Any())
                        {
                            Debug.WriteLine("=== Gestion des hébergements ===");

                            // Récupérer les IDs des hébergements à associer
                            var hebergementIds = voyage.Hebergements.Select(h => h.HebergementId).ToList();
                            Debug.WriteLine($"IDs des hébergements: {string.Join(", ", hebergementIds)}");

                            // Vérifier que tous les hébergements existent dans la base
                            var existingHebergements = await _context.Hebergements
                                .Where(h => hebergementIds.Contains(h.HebergementId))
                                .ToListAsync();

                            Debug.WriteLine($"Hébergements trouvés en DB: {existingHebergements.Count}");

                            if (existingHebergements.Count != hebergementIds.Count)
                            {
                                var foundIds = existingHebergements.Select(h => h.HebergementId).ToList();
                                var missingIds = hebergementIds.Except(foundIds).ToList();
                                throw new InvalidOperationException($"Hébergements non trouvés avec les IDs: {string.Join(", ", missingIds)}");
                            }

                            // Charger le voyage avec sa collection d'hébergements pour établir les relations
                            var voyageWithHebergements = await _context.Voyages
                                .Include(v => v.Hebergements)
                                .FirstAsync(v => v.VoyageId == newVoyage.VoyageId);

                            // Associer les hébergements existants au voyage
                            foreach (var hebergement in existingHebergements)
                            {
                                voyageWithHebergements.Hebergements.Add(hebergement);
                                Debug.WriteLine($"Hébergement associé: {hebergement.Nom} (ID: {hebergement.HebergementId})");
                            }
                        }

                        // 4. Sauvegarder toutes les relations
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        Debug.WriteLine("=== Voyage et relations sauvegardés avec succès ===");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"=== ERREUR dans la transaction ===");
                        Debug.WriteLine($"Message: {ex.Message}");
                        Debug.WriteLine($"Type: {ex.GetType().Name}");
                        Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                        if (ex.InnerException != null)
                        {
                            Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                        }

                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                _cachedVoyages = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"=== ERREUR GLOBALE AddVoyageAsync ===");
                Debug.WriteLine($"Message: {ex.Message}");
                Debug.WriteLine($"Type: {ex.GetType().Name}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Debug.WriteLine($"Inner exception type: {ex.InnerException.GetType().Name}");
                }

                throw;
            }
        }

        // ... rest of the methods remain the same ...
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
                        // Charger l'entité existante avec toutes ses relations
                        var voyage = await _context.Voyages
                            .Include(v => v.Activites)
                            .Include(v => v.Hebergements)
                            .Include(v => v.Utilisateurs) // Inclure les utilisateurs liés
                            .FirstOrDefaultAsync(v => v.VoyageId == voyageId);

                        if (voyage == null)
                        {
                            Debug.WriteLine($"Voyage avec ID {voyageId} non trouvé");
                            return;
                        }

                        Debug.WriteLine($"Suppression du voyage ID: {voyageId}");
                        Debug.WriteLine($"Activités liées: {voyage.Activites?.Count ?? 0}");
                        Debug.WriteLine($"Hébergements liés: {voyage.Hebergements?.Count ?? 0}");
                        Debug.WriteLine($"Utilisateurs liés: {voyage.Utilisateurs?.Count ?? 0}");

                        // 1. Supprimer les relations avec les utilisateurs (OrganisationVoyage)
                        if (voyage.Utilisateurs != null && voyage.Utilisateurs.Any())
                        {
                            voyage.Utilisateurs.Clear();
                            Debug.WriteLine("Relations avec utilisateurs supprimées");
                        }

                        // 2. Supprimer les relations avec les activités (ActiviteVoyage)
                        if (voyage.Activites != null && voyage.Activites.Any())
                        {
                            voyage.Activites.Clear();
                            Debug.WriteLine("Relations avec activités supprimées");
                        }

                        // 3. Supprimer les relations avec les hébergements (HebergementVoyage)
                        if (voyage.Hebergements != null && voyage.Hebergements.Any())
                        {
                            voyage.Hebergements.Clear();
                            Debug.WriteLine("Relations avec hébergements supprimées");
                        }

                        // 4. Sauvegarder la suppression des relations
                        await _context.SaveChangesAsync();
                        Debug.WriteLine("Toutes les relations supprimées et sauvegardées");

                        // 5. Supprimer le voyage lui-même
                        _context.Voyages.Remove(voyage);
                        await _context.SaveChangesAsync();
                        Debug.WriteLine("Voyage supprimé");

                        // 6. Valider la transaction
                        await transaction.CommitAsync();
                        _cachedVoyages = null;

                        Debug.WriteLine($"Voyage {voyageId} supprimé avec succès");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Erreur dans la transaction de suppression: {ex}");
                        await transaction.RollbackAsync();
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de la suppression du voyage {voyageId}: {ex}");

                var errorDetails = new List<string> { ex.Message };

                var currentEx = ex.InnerException;
                while (currentEx != null)
                {
                    errorDetails.Add(currentEx.Message);
                    currentEx = currentEx.InnerException;
                }

                var fullErrorMessage = string.Join(" -> ", errorDetails);
                Debug.WriteLine($"Détails complets de l'erreur: {fullErrorMessage}");

                throw new InvalidOperationException($"Impossible de supprimer le voyage: {fullErrorMessage}", ex);
            }
        }
    }
}