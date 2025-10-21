// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Options;
// using MoviePortal.Models.DTOs;
// using MoviePortal.Models.Entities;
// using MoviePortal.Models.Specification;
// using MoviePortal.Models.ValueObject;
// using MoviePortal.Repositories.Interfaces;
//
// namespace MoviePortal.Services;
//
// public class StorageService(MinioObjectStorage storage, IUnitOfWork uow, IOptions<StorageOptions> opt)
// {
//     public Task<Uri> GetReadSignedUrlAsync(string bucket, string objectKey, TimeSpan ttl, CancellationToken ct = default) 
//         => storage.GetReadSignedUrlAsync(bucket, objectKey, ttl, ct);
//     public async Task<List<ImageAsset>> FindImagesAsync(ImageFilter f, CancellationToken ct = default)
//     {
//         var q = uow.ImageAssets.Query(); // IQueryable<ImageAsset>
//
//         if (f.MovieId.HasValue) q = q.Where(i => i.MovieId == f.MovieId);
//         if (f.EpisodeId.HasValue) q = q.Where(i => i.EpisodeId == f.EpisodeId);
//         if (f.Kind.HasValue) q = q.Where(i => i.Kind == f.Kind);
//         if (!string.IsNullOrWhiteSpace(f.Q))
//             q = q.Where(i => i.ObjectKey.Contains(f.Q!) || i.Alt.Contains(f.Q!));
//
//         return await q.AsNoTracking()
//             .OrderByDescending(i => i.Id)
//             .Take(f.Take)
//             .ToListAsync(ct);
//     }
//
//     public async Task<List<VideoAsset>> FindVideosAsync(VideoFilter f, CancellationToken ct = default)
//     {
//         var q = uow.VideoAssets.Query(); // IQueryable<VideoAsset>
//
//         if (f.MovieId.HasValue) q = q.Where(v => v.MovieId == f.MovieId);
//         if (f.EpisodeId.HasValue) q = q.Where(v => v.EpisodeId == f.EpisodeId);
//         if (f.Kind.HasValue) q = q.Where(v => v.Kind == f.Kind);
//         if (f.Quality.HasValue) q = q.Where(v => v.Quality == f.Quality);
//         if (!string.IsNullOrWhiteSpace(f.Q))
//             q = q.Where(v => v.ObjectKey.Contains(f.Q!) || v.Language.Contains(f.Q!));
//
//         return await q.AsNoTracking()
//             .OrderByDescending(v => v.Id)
//             .Take(f.Take)
//             .ToListAsync(ct);
//     }
//
//     public async Task<ImageAsset> UploadImageAsync(
//         int movieId, Stream file, string fileName, string? contentType,
//         ImageKind kind = ImageKind.Poster, string? alt = null, CancellationToken ct = default)
//     {
//         var movie = await uow.Movies.FindAsync(new object[] { movieId }, ct)
//                     ?? throw new KeyNotFoundException("Movie not found");
//
//         var ext = Path.GetExtension(fileName);
//         var key = $"movies/{movieId}/images/{kind.ToString().ToLowerInvariant()}/{Guid.NewGuid():N}{ext}";
//
//         var put = await storage.UploadAsync(
//             opt.Value.Buckets.Images, file, key, contentType, ct);
//
//         var asset = new ImageAsset
//         {
//             MovieId = movieId,
//             Kind = kind,
//             Alt = alt ?? "",
//             Bucket = put.Bucket,
//             ObjectKey = put.ObjectKey,
//             Endpoint = opt.Value.S3.Endpoint,
//             ContentType = contentType,
//             SizeBytes = put.SizeBytes,
//             ETag = put.ETag,
//             CreatedAt = DateTime.UtcNow
//         };
//
//         await uow.ImageAssets.AddAsync(asset, ct);
//         await uow.SaveChangesAsync(ct);
//         return asset;
//     }
//     
//     public async Task DeleteImageAsync(int imageId, CancellationToken ct = default)
//     {
//         var asset = await uow.ImageAssets.FindAsync(imageId, ct)
//                     ?? throw new KeyNotFoundException("Image not found");
//
//         await storage.DeleteAsync(asset.Bucket, asset.ObjectKey, ct);
//         uow.ImageAssets.Remove(asset);
//         await uow.SaveChangesAsync(ct);
//     }
//
//     public async Task<VideoAsset> UploadVideoAsync(
//         int movieId, Stream file, string fileName, string? contentType,
//         VideoKind kind, QualityLevel quality, string language = "vi",
//         string? subtitles = null, CancellationToken ct = default)
//     {
//         var movie = await uow.Movies.FindAsync(new object[] { movieId }, ct)
//                     ?? throw new KeyNotFoundException("Movie not found");
//
//         var ext = Path.GetExtension(fileName);
//         var key = $"movies/{movieId}/videos/{quality.ToString().ToLowerInvariant()}/{Guid.NewGuid():N}{ext}";
//
//         var put = await storage.UploadAsync(
//             opt.Value.Buckets.Videos, file, key, contentType, ct);
//
//         var asset = new VideoAsset
//         {
//             MovieId = movieId,
//             Kind = kind,
//             Quality = quality,
//             Language = language,
//             Subtitles = subtitles,
//             Bucket = put.Bucket,
//             ObjectKey = put.ObjectKey,
//             Endpoint = opt.Value.S3.Endpoint,
//             ContentType = contentType,
//             SizeBytes = put.SizeBytes,
//             ETag = put.ETag,
//             Status = PublishStatus.Published,
//             CreatedAt = DateTime.UtcNow
//         };
//
//         await uow.VideoAssets.AddAsync(asset, ct);
//         await uow.SaveChangesAsync(ct);
//         return asset;
//     }
//     
//     public async Task DeleteVideoAsync(int videoId, CancellationToken ct = default)
//     {
//         var asset = await uow.VideoAssets.FindAsync(videoId, ct)
//                     ?? throw new KeyNotFoundException("Video not found");
//
//         await storage.DeleteAsync(asset.Bucket, asset.ObjectKey, ct);
//         uow.VideoAssets.Remove(asset);
//         await uow.SaveChangesAsync(ct);
//     }
// }