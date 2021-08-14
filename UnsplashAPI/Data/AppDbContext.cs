using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UnsplashAPI.Modules;

namespace UnsplashAPI.Data {
  public class AppDbContext: IdentityDbContext<User> {
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options) {}

    protected override void OnModelCreating(ModelBuilder builder) {
      base.OnModelCreating(builder);

      builder.Entity<Photo>()
        .HasOne(p => p.Owner)
        .WithMany(u => u.UserPhotos)
        .HasForeignKey(p => p.OwnerId);

      builder.Entity<User>()
        .HasMany(u => u.UserPhotos)
        .WithOne(p => p.Owner)
        .HasForeignKey(p => p.OwnerId);
    }

    public DbSet<Photo> Photos { get; set; }
  }
}