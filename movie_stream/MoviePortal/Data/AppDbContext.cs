using Microsoft.EntityFrameworkCore;
using MoviePortal.Models;
using MoviePortal.Models.Entities;

namespace MoviePortal.Data;

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
    public DbSet<SubtitleAsset> SubtitlesAssets => Set<SubtitleAsset>();

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
        mb.Entity<SubtitleAsset>().ToTable("subtitle_assets");

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

        // Enum mappings
        mb.Entity<Movie>().Property(x => x.Type).HasConversion<int>();
        mb.Entity<Movie>().Property(x => x.Status).HasConversion<int>();
        mb.Entity<Movie>().Property(x => x.Quality).HasConversion<int>();
        mb.Entity<Episode>().Property(x => x.Status).HasConversion<int>();
        mb.Entity<Episode>().Property(x => x.Quality).HasConversion<int>();
        mb.Entity<ImageAsset>().Property(x => x.Kind).HasConversion<int>();
        mb.Entity<VideoAsset>().Property(x => x.Kind).HasConversion<int>();


        // Soft delete filter
        mb.Entity<Movie>().HasQueryFilter(m => !m.IsDeleted);
    }
}