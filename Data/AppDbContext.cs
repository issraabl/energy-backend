using Microsoft.EntityFrameworkCore;
using EnergyTrackerr.Models;
using EnergyTrackerr.Migrations;

namespace EnergyTrackerr.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Site> Sites => Set<Site>();
        public DbSet<Zone> Zones => Set<Zone>();
        public DbSet<Mesure> Mesures => Set<Mesure>();
        public DbSet<Equipement> Equipements => Set<Equipement>();
        public DbSet<Energie> Energies => Set<Energie>();
        public DbSet<Electricite> Electricites => Set<Electricite>();
        public DbSet<Gasoil> Gasoils => Set<Gasoil>();
        public DbSet<Eau> Eaux => Set<Eau>();
        public DbSet<Alertes> Alertes => Set<Alertes>();
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<AnalyseEnergetique> Analyses { get; set; }
        public DbSet<Rapport> Rapports { get; set; }
        public DbSet<Recommandation> Recommandations { get; set; }
        public DbSet<Notfication> Notifications { get; set; }
        public DbSet<Anomalie> Anomalies { get; set; }
        public DbSet<Seuil> Seuils { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Clés primaires
            modelBuilder.Entity<Zone>().HasKey(z => z.IdZone);
            modelBuilder.Entity<Equipement>().HasKey(e => e.IdEquipement);
            modelBuilder.Entity<Energie>().HasKey(e => e.IdEnergie);
            modelBuilder.Entity<Mesure>().HasKey(m => m.IdMesure);
            modelBuilder.Entity<Alertes>().HasKey(a => a.IdAlerte);
            modelBuilder.Entity<Seuil>().HasKey(s => s.IdSeuil);
            modelBuilder.Entity<Site>().HasKey(s => s.IdSite);

            // Relation Mesure → Energie
            modelBuilder.Entity<Mesure>()
                .HasOne(m => m.Energie)
                .WithMany(e => e.Mesures)
                .HasForeignKey(m => m.EnergieId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relation Mesure → Equipement
            modelBuilder.Entity<Mesure>()
                .HasOne(m => m.Equipement)
                .WithMany(e => e.Mesures)
                .HasForeignKey(m => m.EquipementId)
                .OnDelete(DeleteBehavior.SetNull);

            // Relation Alerte → Equipement
            modelBuilder.Entity<Alertes>()
                .HasOne(a => a.Equipement)
                .WithMany(e => e.Alertes)
                .HasForeignKey(a => a.EquipementId)
                .OnDelete(DeleteBehavior.SetNull);

            // TPH Inheritance pour Energie
            modelBuilder.Entity<Energie>()
                .HasDiscriminator<string>("EnergieType")
                .HasValue<Electricite>("Electricite")
                .HasValue<Gasoil>("Gasoil")
            .HasValue<Eau>("Eau");

            modelBuilder.Entity<Notfication>()
    .HasOne(n => n.Utilisateur)
    .WithMany(u => u.Notifications)
    .HasForeignKey(n => n.IdUtilisateur)
    .OnDelete(DeleteBehavior.Cascade);
            base.OnModelCreating(modelBuilder);
        }
    }
}