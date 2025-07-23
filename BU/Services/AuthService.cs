using DAL.DB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using BU.Models;

namespace BU.Services
{
    public class AuthService : IAuthService
    {
        private readonly IVoyageService _voyageService;
        private Utilisateur _utilisateurActuel;

        public bool EstConnecte => _utilisateurActuel != null;

        public AuthService(IVoyageService voyageService)
        {
            _voyageService = voyageService;
        }

        public async Task<bool> Authentifier(string email, string motDePasse)
        {
            var utilisateur = await _voyageService.GetUtilisateurParEmail(email);

            if (utilisateur == null || !VerifierMotDePasse(motDePasse, utilisateur.MotDePasseHash))
                return false;

            _utilisateurActuel = utilisateur;
            return true;
        }

        public async Task<bool> Enregistrer(Utilisateur nouvelUtilisateur)
        {
            nouvelUtilisateur.MotDePasseHash = HasherMotDePasse(nouvelUtilisateur.MotDePasseHash);
            return await _voyageService.AjouterUtilisateur(nouvelUtilisateur);
        }

        public Task<Utilisateur> GetUtilisateurActuel() => Task.FromResult(_utilisateurActuel);

        public Task Deconnecter()
        {
            _utilisateurActuel = null;
            return Task.CompletedTask;
        }

        private string HasherMotDePasse(string motDePasse)
        {
            // Implémentation simplifiée - utiliser une librairie comme BCrypt.Net en production
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(motDePasse));
        }

        private bool VerifierMotDePasse(string motDePasse, string hashStocke)
        {
            return HasherMotDePasse(motDePasse) == hashStocke;
        }
    }
}