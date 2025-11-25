using Microsoft.EntityFrameworkCore;
using NouFlix.Models.Entities;

namespace NouFlix.Persistence.Data;

public class AppDbContext(DbContextOptions<AppDbContext> opt) : DbContext(opt)
{
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Episode> Episodes => Set<Episode>();
    public DbSet<VideoAsset> VideoAssets => Set<VideoAsset>();
    public DbSet<ImageAsset> ImageAssets => Set<ImageAsset>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<MovieGenre> MovieGenres => Set<MovieGenre>();
    public DbSet<Studio> Studios => Set<Studio>();
    public DbSet<MovieStudio> MovieStudios => Set<MovieStudio>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<History> Histories => Set<History>();
    public DbSet<Playlist> Playlists => Set<Playlist>();
    public DbSet<SavedList> SavedLists => Set<SavedList>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<SubtitleAsset> SubtitleAssets => Set<SubtitleAsset>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Tên bảng
        mb.Entity<Movie>().ToTable("movies");
        mb.Entity<Season>().ToTable("seasons");
        mb.Entity<Episode>().ToTable("episodes");
        mb.Entity<VideoAsset>().ToTable("video_assets");
        mb.Entity<ImageAsset>().ToTable("image_assets");
        mb.Entity<Genre>().ToTable("genres");
        mb.Entity<MovieGenre>().ToTable("movie_genres");
        mb.Entity<Studio>().ToTable("studios");
        mb.Entity<MovieStudio>().ToTable("movie_studios");
        mb.Entity<User>().ToTable("users");
        mb.Entity<Profile>().ToTable("profiles");
        mb.Entity<RefreshToken>().ToTable("refresh_tokens");
        mb.Entity<History>().ToTable("histories");
        mb.Entity<Notification>().ToTable("notifications");
        mb.Entity<Playlist>().ToTable("playlists");
        mb.Entity<SavedList>().ToTable("saved_lists");
        mb.Entity<Review>().ToTable("reviews");
        mb.Entity<SubtitleAsset>().ToTable("subtitles_assets");
        mb.Entity<SubscriptionPlan>().ToTable("subscription_plans");
        mb.Entity<UserSubscription>().ToTable("user_subscriptions");
        mb.Entity<Transaction>().ToTable("transactions");

        // Index/unique
        mb.Entity<Movie>().HasIndex(x => x.Title);
        mb.Entity<Movie>().HasIndex(x => x.Slug).IsUnique();
        mb.Entity<Genre>().HasIndex(x => x.Name);
        mb.Entity<Studio>().HasIndex(x => x.Name);

        mb.Entity<Episode>()
            .HasIndex(e => new { e.MovieId, e.Number })
            .IsUnique();

        mb.Entity<Season>()
            .HasIndex(s => new { s.MovieId, s.Number })
            .IsUnique();

        mb.Entity<MovieGenre>()
            .HasIndex(x => new { x.MovieId, x.GenreId })
            .IsUnique();

        mb.Entity<MovieStudio>()
            .HasIndex(x => new { x.MovieId, x.StudioId })
            .IsUnique();
        
        mb.Entity<History>()
            .HasKey(h => new { h.UserId, h.MovieId });
        mb.Entity<History>()
            .HasIndex(h => new { h.UserId, h.MovieId })
            .IsUnique();

        mb.Entity<User>(entity =>
        {
            entity.HasDiscriminator<string>("Role")
                .HasValue<User>("User")
                .HasValue<Admin>("Admin");
            
            entity.OwnsOne(u => u.Email, e =>
            {
                e.Property(p => p.Address)
                    .HasColumnName("Email")
                    .HasMaxLength(256)
                    .IsRequired();

                e.HasIndex(p => p.Address).IsUnique();
            });
            
            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_User_CreatedAt");
        });
        
        mb.Entity<Profile>(b =>
        {
            b.OwnsOne(p => p.Name);
            
            b.HasIndex(e => e.DateOfBirth)
                .HasDatabaseName("IX_User_DateOfBirth");
        });
        
        mb.Entity<RefreshToken>()
            .HasKey(r => new { r.UserId, r.Token });

        mb.Entity<PlaylistItem>()
            .HasKey(x => new { x.PlaylistId, x.MovieId });
        mb.Entity<PlaylistItem>()
            .HasIndex(x => new { x.PlaylistId, x.Position });

        mb.Entity<SavedList>()
            .HasKey(x => new { x.UserId, x.MovieId });
        mb.Entity<SavedList>()
            .HasIndex(x => new { x.UserId, x.AddedAt });
        
        mb.Entity<Review>()
            .HasKey(h => new { h.UserId, h.MovieId });
        mb.Entity<Review>()
            .HasIndex(h => new { h.UserId, h.MovieId })
            .IsUnique();

        // Quan hệ
        mb.Entity<Season>()
            .HasOne< Movie >(s => s.Movie)
            .WithMany(m => m.Seasons)
            .HasForeignKey(s => s.MovieId)
            .OnDelete(DeleteBehavior.Cascade);
        
        mb.Entity<Episode>()
            .HasOne(e => e.Movie).WithMany(m => m.Episodes)
            .HasForeignKey(e => e.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<Episode>()
            .HasOne(e => e.Season).WithMany(s => s.Episodes)
            .HasForeignKey(e => e.SeasonId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<ImageAsset>()
            .HasOne(i => i.Movie).WithMany(m => m.Images)
            .HasForeignKey(i => i.MovieId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<ImageAsset>()
            .HasOne(i => i.Episode).WithMany(e => e.Images)
            .HasForeignKey(i => i.EpisodeId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<VideoAsset>()
            .HasOne(v => v.Movie).WithMany(m => m.Videos)
            .HasForeignKey(v => v.MovieId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<VideoAsset>()
            .HasOne(v => v.Episode).WithMany(e => e.Videos)
            .HasForeignKey(v => v.EpisodeId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<MovieGenre>()
            .HasOne(mg => mg.Movie).WithMany(m => m.MovieGenres)
            .HasForeignKey(mg => mg.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<MovieGenre>()
            .HasOne(mg => mg.Genre).WithMany(g => g.MovieGenres)
            .HasForeignKey(mg => mg.GenreId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<MovieStudio>()
            .HasOne(ms => ms.Movie).WithMany(m => m.MovieStudios)
            .HasForeignKey(ms => ms.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<MovieStudio>()
            .HasOne(ms => ms.Studio).WithMany(s => s.MovieStudios)
            .HasForeignKey(ms => ms.StudioId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<History>()
            .HasOne(e => e.User)
            .WithMany(u => u.Histories)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        mb.Entity<History>()
            .HasOne(e => e.Movie)
            .WithMany(m => m.Histories)
            .HasForeignKey(e => e.MovieId)
            .OnDelete(DeleteBehavior.Cascade);
        
        mb.Entity<User>()
            .HasOne(u => u.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<Profile>(p => p.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        mb.Entity<Notification>()
            .HasOne(c => c.Sender)
            .WithMany(u => u.SenderNotification)
            .HasForeignKey(c => c.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Notification>()
            .HasOne(c => c.Receiver)
            .WithMany(u => u.RecipientNotification)
            .HasForeignKey(c => c.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);
        
        mb.Entity<PlaylistItem>()
            .HasOne(x => x.Playlist)
            .WithMany(p => p.Items)
            .HasForeignKey(x => x.PlaylistId)
            .OnDelete(DeleteBehavior.Cascade);
        
        mb.Entity<Review>()
            .HasOne(e => e.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        mb.Entity<Review>()
            .HasOne(e => e.Movie)
            .WithMany(m => m.Reviews)
            .HasForeignKey(e => e.MovieId)
            .OnDelete(DeleteBehavior.Cascade);
        
        mb.Entity<SavedList>()
            .HasOne(e => e.User)
            .WithMany(u => u.SavedList)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        mb.Entity<SavedList>()
            .HasOne(e => e.Movie)
            .WithMany(m => m.SavedList)
            .HasForeignKey(e => e.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        // Enum mappings
        mb.Entity<Movie>().Property(x => x.Type).HasConversion<int>();
        mb.Entity<Movie>().Property(x => x.Status).HasConversion<int>();
        mb.Entity<Movie>().Property(x => x.Quality).HasConversion<int>();
        mb.Entity<Episode>().Property(x => x.Status).HasConversion<int>();
        mb.Entity<Episode>().Property(x => x.Quality).HasConversion<int>();
        mb.Entity<ImageAsset>().Property(x => x.Kind).HasConversion<int>();

        mb.Entity<Episode>()
            .HasIndex(e => new { e.MovieId, e.Number })
            .IsUnique();

        mb.Entity<Season>()
            .HasIndex(s => new { s.MovieId, s.Number })
            .IsUnique();

        mb.Entity<MovieGenre>()
            .HasIndex(x => new { x.MovieId, x.GenreId })
            .IsUnique();

        mb.Entity<MovieStudio>()
            .HasIndex(x => new { x.MovieId, x.StudioId })
            .IsUnique();
        
        mb.Entity<History>()
            .HasKey(h => new { h.UserId, h.MovieId });
        mb.Entity<History>()
            .HasIndex(h => new { h.UserId, h.MovieId })
            .IsUnique();

        mb.Entity<User>(entity =>
        {
            entity.HasDiscriminator<string>("Role")
                .HasValue<User>("User")
                .HasValue<Admin>("Admin");
            
            entity.OwnsOne(u => u.Email, e =>
            {
                e.Property(p => p.Address)
                    .HasColumnName("Email")
                    .HasMaxLength(256)
                    .IsRequired();

                e.HasIndex(p => p.Address).IsUnique();
            });
            
            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_User_CreatedAt");
        });
        
        mb.Entity<Profile>(b =>
        {
            b.OwnsOne(p => p.Name);
            
            b.HasIndex(e => e.DateOfBirth)
                .HasDatabaseName("IX_User_DateOfBirth");
        });
        
        mb.Entity<RefreshToken>()
            .HasKey(r => new { r.UserId, r.Token });

        mb.Entity<PlaylistItem>()
            .HasKey(x => new { x.PlaylistId, x.MovieId });
        mb.Entity<PlaylistItem>()
            .HasIndex(x => new { x.PlaylistId, x.Position });

        mb.Entity<SavedList>()
            .HasKey(x => new { x.UserId, x.MovieId });
        mb.Entity<SavedList>()
            .HasIndex(x => new { x.UserId, x.AddedAt });
        
        mb.Entity<Review>()
            .HasKey(h => new { h.UserId, h.MovieId });
        mb.Entity<Review>()
            .HasIndex(h => new { h.UserId, h.MovieId })
            .IsUnique();

        // Quan hệ
        mb.Entity<Season>()
            .HasOne< Movie >(s => s.Movie)
            .WithMany(m => m.Seasons)
            .HasForeignKey(s => s.MovieId)
            .OnDelete(DeleteBehavior.Cascade);
        
        mb.Entity<Episode>()
            .HasOne(e => e.Movie).WithMany(m => m.Episodes)
            .HasForeignKey(e => e.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<Episode>()
            .HasOne(e => e.Season).WithMany(s => s.Episodes)
            .HasForeignKey(e => e.SeasonId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<ImageAsset>()
            .HasOne(i => i.Movie).WithMany(m => m.Images)
            .HasForeignKey(i => i.MovieId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<ImageAsset>()
            .HasOne(i => i.Episode).WithMany(e => e.Images)
            .HasForeignKey(i => i.EpisodeId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<VideoAsset>()
            .HasOne(v => v.Movie).WithMany(m => m.Videos)
            .HasForeignKey(v => v.MovieId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<VideoAsset>()
            .HasOne(v => v.Episode).WithMany(e => e.Videos)
            .HasForeignKey(v => v.EpisodeId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<MovieGenre>()
            .HasOne(mg => mg.Movie).WithMany(m => m.MovieGenres)
            .HasForeignKey(mg => mg.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<MovieGenre>()
            .HasOne(mg => mg.Genre).WithMany(g => g.MovieGenres)
            .HasForeignKey(mg => mg.GenreId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<MovieStudio>()
            .HasOne(ms => ms.Movie).WithMany(m => m.MovieStudios)
            .HasForeignKey(ms => ms.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<MovieStudio>()
            .HasOne(ms => ms.Studio).WithMany(s => s.MovieStudios)
            .HasForeignKey(ms => ms.StudioId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<History>()
            .HasOne(e => e.User)
            .WithMany(u => u.Histories)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        mb.Entity<History>()
            .HasOne(e => e.Movie)
            .WithMany(m => m.Histories)
            .HasForeignKey(e => e.MovieId)
            .OnDelete(DeleteBehavior.Cascade);
        
        mb.Entity<User>()
            .HasOne(u => u.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<Profile>(p => p.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        mb.Entity<Notification>()
            .HasOne(c => c.Sender)
            .WithMany(u => u.SenderNotification)
            .HasForeignKey(c => c.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Notification>()
            .HasOne(c => c.Receiver)
            .WithMany(u => u.RecipientNotification)
            .HasForeignKey(c => c.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);
        
        mb.Entity<PlaylistItem>()
            .HasOne(x => x.Playlist)
            .WithMany(p => p.Items)
            .HasForeignKey(x => x.PlaylistId)
            .OnDelete(DeleteBehavior.Cascade);
        
        mb.Entity<Review>()
            .HasOne(e => e.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        mb.Entity<Review>()
            .HasOne(e => e.Movie)
            .WithMany(m => m.Reviews)
            .HasForeignKey(e => e.MovieId)
            .OnDelete(DeleteBehavior.Cascade);
        
        mb.Entity<SavedList>()
            .HasOne(e => e.User)
            .WithMany(u => u.SavedList)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        mb.Entity<SavedList>()
            .HasOne(e => e.Movie)
            .WithMany(m => m.SavedList)
            .HasForeignKey(e => e.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        // Enum mappings
        mb.Entity<Movie>().Property(x => x.Type).HasConversion<int>();
        mb.Entity<Movie>().Property(x => x.Status).HasConversion<int>();
        mb.Entity<Movie>().Property(x => x.Quality).HasConversion<int>();
        mb.Entity<Episode>().Property(x => x.Status).HasConversion<int>();
        mb.Entity<Episode>().Property(x => x.Quality).HasConversion<int>();
        mb.Entity<ImageAsset>().Property(x => x.Kind).HasConversion<int>();
        mb.Entity<VideoAsset>().Property(x => x.Kind).HasConversion<int>();
        mb.Entity<Profile>().Property(x => x.Gender).HasConversion<int>();
        mb.Entity<PlaylistItem>().Property(x => x.Position).IsRequired();
        mb.Entity<Review>().Property(x => x.Number).IsRequired();
        
        mb.Entity<SubscriptionPlan>()
            .Property(p => p.Features)
            .HasConversion(
                // Model -> Provider (List<string> -> string)
                v => System.Text.Json.JsonSerializer.Serialize(
                    v ?? new List<string>(),
                    (System.Text.Json.JsonSerializerOptions?)null
                ),
                // Provider -> Model (string -> List<string>)
                v => string.IsNullOrWhiteSpace(v)
                    ? new List<string>()
                    : System.Text.Json.JsonSerializer.Deserialize<List<string>>(
                        v,
                        (System.Text.Json.JsonSerializerOptions?)null
                    ) ?? new List<string>()
            );

        mb.Entity<SubscriptionPlan>().Property(x => x.Type).HasConversion<int>();
        mb.Entity<UserSubscription>().Property(x => x.Status).HasConversion<int>();
        mb.Entity<Transaction>().Property(x => x.Status).HasConversion<int>();

        // Soft delete filter
        mb.Entity<Movie>().HasQueryFilter(m => !m.IsDeleted);
    }
}