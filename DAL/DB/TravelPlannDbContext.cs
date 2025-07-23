using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DAL.DB;

public partial class TravelPlannDbContext : DbContext
{
    public TravelPlannDbContext()
    {
    }

    public TravelPlannDbContext(DbContextOptions<TravelPlannDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Activite> Activites { get; set; }

    public virtual DbSet<ClassementVoyageur> ClassementVoyageurs { get; set; }

    public virtual DbSet<GroupeVoyage> GroupeVoyages { get; set; }

    public virtual DbSet<Hebergement> Hebergements { get; set; }

    public virtual DbSet<MembreGroupe> MembreGroupes { get; set; }

    public virtual DbSet<NiveauRecompense> NiveauRecompenses { get; set; }

    public virtual DbSet<PointsRecompense> PointsRecompenses { get; set; }

    public virtual DbSet<ReservationHebergement> ReservationHebergements { get; set; }

    public virtual DbSet<Utilisateur> Utilisateurs { get; set; }

    public virtual DbSet<Voyage> Voyages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost,1433;Database=TravelPlanner;User Id=sa;Password=1235OHdf%e;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
       
        // Configuration des relations many-to-many avec cascade delete
    modelBuilder.Entity<Voyage>()
        .HasMany(v => v.Activites)
        .WithMany(a => a.Voyages)
        .UsingEntity<Dictionary<string, object>>(
            "ActiviteVoyage",
            j => j.HasOne<Activite>().WithMany().HasForeignKey("ActiviteId").OnDelete(DeleteBehavior.Cascade),
            j => j.HasOne<Voyage>().WithMany().HasForeignKey("VoyageId").OnDelete(DeleteBehavior.Cascade),
            j =>
            {
                j.HasKey("VoyageId", "ActiviteId");
                j.ToTable("ActiviteVoyage");
            });

    modelBuilder.Entity<Voyage>()
        .HasMany(v => v.Hebergements)
        .WithMany(h => h.Voyages)
        .UsingEntity<Dictionary<string, object>>(
            "HebergementVoyage",
            j => j.HasOne<Hebergement>().WithMany().HasForeignKey("HebergementId").OnDelete(DeleteBehavior.Cascade),
            j => j.HasOne<Voyage>().WithMany().HasForeignKey("VoyageId").OnDelete(DeleteBehavior.Cascade),
            j =>
            {
                j.HasKey("VoyageId", "HebergementId");
                j.ToTable("HebergementVoyage");
            });

        modelBuilder.Entity<Activite>(entity =>
        {
            entity.HasKey(e => e.ActiviteId).HasName("PK__Activite__BE3FB865F23B2E86");

            entity.ToTable("Activite");

            entity.Property(e => e.ActiviteId).HasColumnName("ActiviteID");
            entity.Property(e => e.Nom).HasMaxLength(100);

            entity.HasMany(d => d.Voyages).WithMany(p => p.Activites)
                .UsingEntity<Dictionary<string, object>>(
                    "ActiviteVoyage",
                    r => r.HasOne<Voyage>().WithMany()
                        .HasForeignKey("VoyageId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ActiviteVoyage_Voyage"),
                    l => l.HasOne<Activite>().WithMany()
                        .HasForeignKey("ActiviteId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ActiviteVoyage_Activite"),
                    j =>
                    {
                        j.HasKey("ActiviteId", "VoyageId");
                        j.ToTable("ActiviteVoyage");
                        j.IndexerProperty<int>("ActiviteId").HasColumnName("ActiviteID");
                        j.IndexerProperty<int>("VoyageId").HasColumnName("VoyageID");
                    });
        });

        modelBuilder.Entity<ClassementVoyageur>(entity =>
        {
            entity.HasKey(e => e.ClassementId).HasName("PK__Classeme__63F085DD50156D31");

            entity.ToTable("ClassementVoyageur");

            entity.Property(e => e.ClassementId).HasColumnName("ClassementID");
            entity.Property(e => e.DistanceTotale).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UtilisateurId).HasColumnName("UtilisateurID");

            entity.HasOne(d => d.Utilisateur).WithMany(p => p.ClassementVoyageurs)
                .HasForeignKey(d => d.UtilisateurId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Classement_Utilisateur");
        });

        modelBuilder.Entity<GroupeVoyage>(entity =>
        {
            entity.HasKey(e => e.GroupeId).HasName("PK__GroupeVo__5C811B3078FA0CDF");

            entity.ToTable("GroupeVoyage");

            entity.Property(e => e.GroupeId).HasColumnName("GroupeID");
            entity.Property(e => e.DateCreation).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.NomGroupe).HasMaxLength(100);
        });

        modelBuilder.Entity<Hebergement>(entity =>
        {
            entity.HasKey(e => e.HebergementId).HasName("PK__Hebergem__35A3F6B1A87E8D30");

            entity.ToTable("Hebergement");

            entity.Property(e => e.HebergementId).HasColumnName("HebergementID");
            entity.Property(e => e.Cout).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Nom).HasMaxLength(100);
            entity.Property(e => e.TypeHebergement).HasMaxLength(50);
        });

        modelBuilder.Entity<MembreGroupe>(entity =>
        {
            entity.HasKey(e => e.MembreGroupeId).HasName("PK__MembreGr__DED3D73B96357064");

            entity.ToTable("MembreGroupe");

            entity.Property(e => e.MembreGroupeId).HasColumnName("MembreGroupeID");
            entity.Property(e => e.DateAdhesion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.GroupeId).HasColumnName("GroupeID");
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.UtilisateurId).HasColumnName("UtilisateurID");

            entity.HasOne(d => d.Groupe).WithMany(p => p.MembreGroupes)
                .HasForeignKey(d => d.GroupeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Membre_Groupe");

            entity.HasOne(d => d.Utilisateur).WithMany(p => p.MembreGroupes)
                .HasForeignKey(d => d.UtilisateurId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Membre_Utilisateur");
        });

        modelBuilder.Entity<NiveauRecompense>(entity =>
        {
            entity.HasKey(e => e.NiveauRecompenseId).HasName("PK__NiveauRe__04A74635554B36E3");

            entity.ToTable("NiveauRecompense");

            entity.Property(e => e.NiveauRecompenseId).HasColumnName("NiveauRecompenseID");
            entity.Property(e => e.NomNiveau).HasMaxLength(50);
        });

        modelBuilder.Entity<PointsRecompense>(entity =>
        {
            entity.HasKey(e => e.PointsRecompenseId).HasName("PK__PointsRe__9A7FC267F35BD8D3");

            entity.ToTable("PointsRecompense");

            entity.Property(e => e.PointsRecompenseId).HasColumnName("PointsRecompenseID");
            entity.Property(e => e.DateObtention).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.NiveauRecompenseId).HasColumnName("NiveauRecompenseID");
            entity.Property(e => e.UtilisateurId).HasColumnName("UtilisateurID");

            entity.HasOne(d => d.NiveauRecompense).WithMany(p => p.PointsRecompenses)
                .HasForeignKey(d => d.NiveauRecompenseId)
                .HasConstraintName("FK_Points_NiveauRecompense");

            entity.HasOne(d => d.Utilisateur).WithMany(p => p.PointsRecompensesNavigation)
                .HasForeignKey(d => d.UtilisateurId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Points_Utilisateur");
        });

        modelBuilder.Entity<ReservationHebergement>(entity =>
        {
            entity.HasKey(e => e.ReservationId).HasName("PK__Reservat__B7EE5F04BCBEFAA4");

            entity.ToTable("ReservationHebergement");

            entity.Property(e => e.ReservationId).HasColumnName("ReservationID");
            entity.Property(e => e.HebergementId).HasColumnName("HebergementID");
            entity.Property(e => e.NumConfirmation).HasMaxLength(50);
            entity.Property(e => e.StatutReservation).HasMaxLength(50);

            entity.HasOne(d => d.Hebergement).WithMany(p => p.ReservationHebergements)
                .HasForeignKey(d => d.HebergementId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reservation_Hebergement");
        });

        modelBuilder.Entity<Utilisateur>(entity =>
        {
            entity.HasKey(e => e.UtilisateurId).HasName("PK__Utilisat__6CB6AE1F1218C985");

            entity.ToTable("Utilisateur");

            entity.HasIndex(e => e.Email, "UQ_Utilisateur_Email").IsUnique();

            entity.Property(e => e.UtilisateurId).HasColumnName("UtilisateurID");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.MotDePasse).HasMaxLength(255);
            entity.Property(e => e.Nom).HasMaxLength(100);
            entity.Property(e => e.Prenom).HasMaxLength(100);

            entity.HasMany(d => d.Voyages).WithMany(p => p.Utilisateurs)
                .UsingEntity<Dictionary<string, object>>(
                    "OrganisationVoyage",
                    r => r.HasOne<Voyage>().WithMany()
                        .HasForeignKey("VoyageId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_OrganisationVoyage_Voyage"),
                    l => l.HasOne<Utilisateur>().WithMany()
                        .HasForeignKey("UtilisateurId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_OrganisationVoyage_Utilisateur"),
                    j =>
                    {
                        j.HasKey("UtilisateurId", "VoyageId");
                        j.ToTable("OrganisationVoyage");
                        j.IndexerProperty<int>("UtilisateurId").HasColumnName("UtilisateurID");
                        j.IndexerProperty<int>("VoyageId").HasColumnName("VoyageID");
                    });
        });

        modelBuilder.Entity<Voyage>(entity =>
        {
            entity.HasKey(e => e.VoyageId).HasName("PK__Voyage__577D73A343C0B05F");

            entity.ToTable("Voyage");

            entity.Property(e => e.VoyageId).HasColumnName("VoyageID");
            entity.Property(e => e.NomVoyage).HasMaxLength(100);

            entity.HasMany(d => d.Hebergements).WithMany(p => p.Voyages)
                .UsingEntity<Dictionary<string, object>>(
                    "HebergementVoyage",
                    r => r.HasOne<Hebergement>().WithMany()
                        .HasForeignKey("HebergementId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_HebergementVoyage_Hebergement"),
                    l => l.HasOne<Voyage>().WithMany()
                        .HasForeignKey("VoyageId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_HebergementVoyage_Voyage"),
                    j =>
                    {
                        j.HasKey("VoyageId", "HebergementId");
                        j.ToTable("HebergementVoyage");
                        j.IndexerProperty<int>("VoyageId").HasColumnName("VoyageID");
                        j.IndexerProperty<int>("HebergementId").HasColumnName("HebergementID");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
