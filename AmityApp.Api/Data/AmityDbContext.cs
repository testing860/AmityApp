using AmityApp.Api.Data.Entities;
using AmityApp.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AmityApp.Api.Data;
    public class AmityDbContext : DbContext
    {
        public AmityDbContext(DbContextOptions<AmityDbContext> options) : base(options)
        {
        }

    public DbSet<User> Users { get; set; }
    public DbSet<Cordial> Cordials { get; set; }
    public DbSet<Chime> Chimes { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Crown> Crowns { get; set; }
    public DbSet<Candle> Candles { get; set; }
    public DbSet<Connection> Connections { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CordialDto>().HasNoKey().ToTable(tb => tb.ExcludeFromMigrations());

        modelBuilder.Entity<Candle>(e =>
        {
            e.HasKey(c => new { c.CordialId, c.UserId });

            e.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

        });

        modelBuilder.Entity<Crown>(e =>
        {
            e.HasKey(c => new { c.CordialId, c.UserId });

            e.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Chime>()
            .HasOne(c => c.User)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Chime>()
            .HasOne(c => c.Cordial)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Connection>(e =>
        {
            e.HasOne(c => c.RequesterUser)
                .WithMany()
                .HasForeignKey(c => c.RequesterUserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.AccepterUser)
                .WithMany()
                .HasForeignKey(c => c.AccepterUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

}
