using System.Diagnostics;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MoviePortal.Data;
using MoviePortal.Models.Entities;
using MoviePortal.Models.Specification;
using MoviePortal.Models.ValueObject;
using MoviePortal.Services;

namespace MoviePortal.Adapters;

public class FfmpegHlsTranscoder(
    MinioObjectStorage storage,
    AppDbContext db,
    IOptions<StorageOptions> opt,
    IOptions<FfmpegOptions> ffopts)
{
    private static string Posix(string path) => path.Replace("\\", "/");

    private string ResolveFfmpegPath()
    {
        // Ưu tiên: cấu hình -> ENV -> "ffmpeg"
        var p = ffopts.Value.Path
                ?? Environment.GetEnvironmentVariable("FFMPEG_PATH")
                ?? "ffmpeg";

        // Windows: nếu path tuyệt đối mà chưa có .exe
        if (OperatingSystem.IsWindows()
            && Path.IsPathRooted(p)
            && string.IsNullOrWhiteSpace(Path.GetExtension(p)))
        {
            p += ".exe";
        }

        if (Path.IsPathRooted(p) && !File.Exists(p))
            throw new FileNotFoundException($"Không tìm thấy ffmpeg: {p}");

        return p;
    }

    private string ResolveFfprobePath()
    {
        // ffprobe cùng thư mục với ffmpeg (hoặc đặt ENV FFMPEG_PATH chuẩn)
        var ffmpegPath = ResolveFfmpegPath();
        var dir = Path.GetDirectoryName(ffmpegPath)!;
        var probe = Path.Combine(dir, OperatingSystem.IsWindows() ? "ffprobe.exe" : "ffprobe");
        if (!File.Exists(probe))
            throw new FileNotFoundException($"Không tìm thấy ffprobe: {probe}");
        return probe;
    }

    private async Task<bool> HasAudioAsync(string ffprobePath, string inputPath, CancellationToken ct)
    {
        var psi = new ProcessStartInfo
        {
            FileName = ffprobePath,
            Arguments = $"-v error -select_streams a -show_entries stream=index -of csv=p=0 \"{inputPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var p = Process.Start(psi)!;
        var stdout = await p.StandardOutput.ReadToEndAsync(ct);
        await p.WaitForExitAsync(ct);
        return !string.IsNullOrWhiteSpace(stdout);
    }

    private ProcessStartInfo Psi(string fileName, string args, string cwd) => new()
    {
        FileName = fileName,
        Arguments = args,
        WorkingDirectory = cwd,
        RedirectStandardError = true,
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    private static string VideoBitrate(string profile) => profile switch
    {
        "1080" => "5000k",
        "720" => "2800k",
        _ => "1400k"
    };
    
    public async Task<(VideoAsset Master, List<VideoAsset> Variants)> TranscodeAndUploadAsync(
        int movieId,
        int? episodeId,
        int? episodeNumber,
        IBrowserFile file,
        string[] profiles,
        string language = "vi",
        CancellationToken ct = default)
    {
        // 1) Input tạm
        var ffmpegPath = ResolveFfmpegPath();
        var ffprobePath = ResolveFfprobePath();

        var tmpRoot = Path.Combine(Path.GetTempPath(), "hls_work", Guid.NewGuid().ToString("N"));
        var outRoot = Path.Combine(tmpRoot, "out");
        Directory.CreateDirectory(tmpRoot);
        Directory.CreateDirectory(outRoot);

        var inputPath = Path.Combine(tmpRoot, file.Name);
        await using (var fs = File.Create(inputPath))
        await using (var src = file.OpenReadStream(10L * 1024 * 1024 * 1024, ct))
            await src.CopyToAsync(fs, ct);

        var ps = (profiles?.Length > 0 ? profiles : new[] { "1080", "720", "480" })
                 .OrderByDescending(p => int.TryParse(p, out var n) ? n : 0)
                 .ToArray();

        // Kiểm tra có audio
        var hasAudio = await HasAudioAsync(ffprobePath, inputPath, ct);

        // Filter: split + scale theo profile
        var splitLabels = string.Concat(ps.Select((_, i) => $"[v{i}]"));
        var filter =
            $"[0:v]split={ps.Length}{splitLabels};" +
            string.Join(';', ps.Select((p, i) => $"[v{i}]scale=-2:{p}[vo{i}]"));

        // Map cho từng output (và audio nếu có)
        var mapPairs = string.Join(' ', Enumerable.Range(0, ps.Length)
            .Select(i => hasAudio ? $"-map [vo{i}] -map 0:a:0" : $"-map [vo{i}]"));

        // var_stream_map khớp với thứ tự map ở (4)
        var vmap = string.Join(' ', Enumerable.Range(0, ps.Length)
            .Select(i => hasAudio ? $"v:{i},a:{i}" : $"v:{i}"));

        // Codec/bitrate
        var encV = string.Join(' ', ps.Select((p, i) =>
            $"-c:v:{i} libx264 -preset veryfast -g 48 -keyint_min 48 -b:v:{i} {VideoBitrate(p)}"));

        // Đường dẫn mẫu (dùng slash kiểu POSIX cho HLS)
        var segPattern = Posix(Path.Combine(outRoot, "%v", "seg_%06d.ts"));
        var outPattern = Posix(Path.Combine(outRoot, "%v", "index.m3u8"));
        const string masterName = "master.m3u8";

        // Chạy ffmpeg
        var args =
            $"-y -i \"{inputPath}\" " +
            $"-filter_complex \"{filter}\" " +
            $"{mapPairs} " +
            "-c:a aac -ac 2 -ar 48000 -b:a 128k " +
            $"{encV} " +
            "-f hls -hls_time 6 -hls_playlist_type vod -hls_flags independent_segments " +
            $"-hls_segment_filename \"{segPattern}\" -master_pl_name \"{masterName}\" " +
            $"-var_stream_map \"{vmap}\" \"{outPattern}\"";

        using (var proc = Process.Start(Psi(ffmpegPath, args, tmpRoot))!)
        {
            var err = await proc.StandardError.ReadToEndAsync(ct);
            await proc.WaitForExitAsync(ct);
            if (proc.ExitCode != 0)
                throw new InvalidOperationException("ffmpeg failed: " + err);
        }

        // Upload lên MinIO (LƯU INDEX Ở ĐÂY)
        var bucket = opt.Value.Buckets.Videos ?? "videos";
        var basePrefix = episodeId is null
            ? $"hls/movies/{movieId}"
            : $"hls/movies/{movieId}/ep{episodeNumber}";

        // master.m3u8
        var masterKey = $"{basePrefix}/master.m3u8";
        await using (var ms = File.OpenRead(Path.Combine(outRoot, masterName)))
            await storage.UploadAsync(bucket, ms, masterKey, "application/vnd.apple.mpegurl", ct);

        // Mỗi profile: index.m3u8 + tất cả *.ts
        for (int i = 0; i < ps.Length; i++)
        {
            var profile = ps[i];

            // FFmpeg tạo thư mục theo index 0/1/2 cho %v
            // -> đọc local theo index, nhưng khi upload lên MinIO, vẫn dùng tên profile (1080/720/480)
            var localDirByIndex = Path.Combine(outRoot, i.ToString());
            var localDir = Directory.Exists(localDirByIndex)
                ? localDirByIndex
                : Path.Combine(outRoot, profile); // fallback nếu về sau bạn đổi sang name:

            var localIndex = Path.Combine(localDir, "index.m3u8");
            if (!File.Exists(localIndex))
                throw new DirectoryNotFoundException($"FFmpeg output missing: {localIndex}");

            // key trên MinIO theo profile
            var indexKey = $"{basePrefix}/{profile}/index.m3u8";

            await using (var ims = File.OpenRead(localIndex))
                await storage.UploadAsync(bucket, ims, indexKey, "application/vnd.apple.mpegurl", ct);

            foreach (var segFile in Directory.EnumerateFiles(localDir, "seg_*.ts"))
            {
                var segName = Path.GetFileName(segFile);
                var segKey  = $"{basePrefix}/{profile}/{segName}";
                await using var sfs = File.OpenRead(segFile);
                await storage.UploadAsync(bucket, sfs, segKey, "video/mp2t", ct);
            }
        }
        
        var created = new List<VideoAsset>();

        var master = new VideoAsset
        {
            MovieId = movieId,
            EpisodeId = episodeId,
            Kind = VideoKind.Master,
            Quality = QualityLevel.High,
            Language = language,
            Bucket = bucket,
            ObjectKey = masterKey,
            Endpoint = opt.Value.S3.Endpoint,
            ContentType= "application/vnd.apple.mpegurl",
            Status = PublishStatus.Published
        };
        created.Add(master);

        foreach (var p in ps)
        {
            var v = new VideoAsset
            {
                MovieId = movieId,
                EpisodeId = episodeId,
                Kind = VideoKind.Variant,
                Quality = Enum.TryParse<QualityLevel>(p, true, out var q) ? q : QualityLevel.Medium,
                Language = language,
                Bucket = bucket,
                ObjectKey = $"{basePrefix}/{p}/index.m3u8",
                Endpoint = opt.Value.S3.Endpoint,
                ContentType= "application/vnd.apple.mpegurl",
                Status = PublishStatus.Published
            };
            created.Add(v);
        }

        db.VideoAssets.AddRange(created);
        await db.SaveChangesAsync(ct);

        // 11) Dọn tạm
        try { Directory.Delete(tmpRoot, true); } catch { /* ignore */ }

        return (master, created.Where(x => x.Kind == VideoKind.Variant).ToList());
    }

    // Subtitle: VTT -> HLS (index + seg) + cập nhật master
    public async Task<SubtitleAsset> TranscodeSubtitleTextToHlsAndAttachAsync(
        int movieId,
        int? episodeId,
        string vttText,
        string language = "vi",
        string label = "Tiếng Việt",
        int hlsSegmentSeconds = 6,
        CancellationToken ct = default)
    {
        // Tìm master để lấy prefix
        var master = await db.VideoAssets.AsNoTracking()
            .Where(x => x.MovieId == movieId && x.EpisodeId == episodeId && x.Kind == VideoKind.Master)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(ct);

        if (master is null) throw new InvalidOperationException("Chưa có master.m3u8");

        var bucket = opt.Value.Buckets.Videos ?? "videos";
        var basePrefix = master.ObjectKey[..master.ObjectKey.LastIndexOf("/master.m3u8", StringComparison.Ordinal)];
        var subPrefix  = $"{basePrefix}/sub/{language}";

        // Viết VTT vào file tạm
        var tmpRoot = Path.Combine(Path.GetTempPath(), "hls_sub_text", Guid.NewGuid().ToString("N"));
        var outDir = Path.Combine(tmpRoot, "out");
        Directory.CreateDirectory(tmpRoot);
        Directory.CreateDirectory(outDir);

        var inputPath = Path.Combine(tmpRoot, "input.vtt");
        await File.WriteAllTextAsync(inputPath, vttText, System.Text.Encoding.UTF8, ct);

        // ffmpeg: segment VTT -> HLS
        var ffmpegPath = ResolveFfmpegPath();
        var segPattern = Posix(Path.Combine(outDir, "seg_%06d.vtt"));
        var subIndex   = Path.Combine(outDir, "index.m3u8");

        var args =
            $"-y -i \"{inputPath}\" -c:s webvtt -f hls " +
            $"-hls_time {hlsSegmentSeconds} -hls_flags independent_segments " +
            $"-hls_segment_type vtt -hls_segment_filename \"{segPattern}\" \"{subIndex}\"";

        using (var proc = Process.Start(Psi(ffmpegPath, args, tmpRoot))!)
        {
            var err = await proc.StandardError.ReadToEndAsync(ct);
            await proc.WaitForExitAsync(ct);
            if (proc.ExitCode != 0)
                throw new InvalidOperationException("ffmpeg subtitle failed: " + err);
        }

        // Upload subtitle index + segs (LƯU INDEX SUBTITLE Ở ĐÂY)
        var subIndexKey = $"{subPrefix}/index.m3u8";
        await using (var ims = File.OpenRead(subIndex))
            await storage.UploadAsync(bucket, ims, subIndexKey, "application/vnd.apple.mpegurl", ct);

        foreach (var segFile in Directory.EnumerateFiles(outDir, "seg_*.vtt"))
        {
            var name = Path.GetFileName(segFile);
            var key  = $"{subPrefix}/{name}";
            await using var seg = File.OpenRead(segFile);
            await storage.UploadAsync(bucket, seg, key, "text/vtt; charset=utf-8", ct);
        }

        // Cập nhật master để khai báo GROUP SUBTITLES
        var masterText = await storage.DownloadTextAsync(bucket, master.ObjectKey, ct);
        var lines = new List<string>(masterText.Replace("\r\n", "\n").Split('\n'));

        var mediaLine =
            $"#EXT-X-MEDIA:TYPE=SUBTITLES,GROUP-ID=\"subs\",NAME=\"{label}\"," +
            $"DEFAULT={(language == "vi" ? "YES" : "NO")}," +
            $"AUTOSELECT=YES,LANGUAGE=\"{language}\",URI=\"sub/{language}/index.m3u8\"";

        if (!lines.Contains(mediaLine))
        {
            // nhét MEDIA ngay sau các header đầu
            var insertAt = 1;
            for (int i = 1; i < Math.Min(lines.Count, 5); i++)
                if (lines[i].StartsWith("#EXT-X-VERSION", StringComparison.OrdinalIgnoreCase))
                    insertAt = i + 1;
            lines.Insert(insertAt, mediaLine);
        }

        for (int i = 0; i < lines.Count; i++)
        {
            var l = lines[i];
            if (!l.StartsWith("#EXT-X-STREAM-INF:", StringComparison.OrdinalIgnoreCase)) continue;
            if (l.Contains("SUBTITLES=\"")) continue;
            lines[i] = l + ",SUBTITLES=\"subs\"";
        }

        var newMaster = string.Join("\n", lines);
        await storage.UploadTextAsync(bucket, master.ObjectKey, newMaster, "application/vnd.apple.mpegurl", ct);

        // 5) Lưu asset phụ đề
        var subAsset = new SubtitleAsset
        {
            MovieId = movieId,
            EpisodeId = episodeId,
            Language = language,
            Label = label,
            Bucket = bucket,
            ObjectKey = $"{basePrefix}/sub/{language}/index.m3u8",
            Endpoint = opt.Value.S3.Endpoint
        };
        db.Set<SubtitleAsset>().Add(subAsset);
        await db.SaveChangesAsync(ct);

        // 6) Dọn tạm
        try { Directory.Delete(tmpRoot, true); } catch { /* ignore */ }

        return subAsset;
    }

    // Upload VTT "raw" (không segment). Không sửa master.
    public async Task<SubtitleAsset> UploadSubtitleVttAsync(
        int movieId,
        int? episodeId,
        int? episodeNumber,
        IBrowserFile file,
        string language = "vi",
        string label = "Tiếng Việt",
        CancellationToken ct = default)
    {
        if (!Path.GetExtension(file.Name).Equals(".vtt", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Subtitle phải là .vtt (nếu .srt, hãy convert trước).");

        var bucket = opt.Value.Buckets.Videos ?? "videos";
        var key = episodeId is null
            ? $"hls/movies/{movieId}/sub/{language}/index.vtt"
            : $"hls/movies/{movieId}/ep{episodeNumber}/sub/{language}/index.vtt";

        await using var fs = file.OpenReadStream(100 * 1024 * 1024, ct);
        await storage.UploadAsync(bucket, fs, key, "text/vtt", ct);

        var sub = new SubtitleAsset
        {
            MovieId = movieId,
            EpisodeId = episodeId,
            Language = language,
            Label = label,
            Bucket = bucket,
            ObjectKey = key,
            Endpoint = opt.Value.S3.Endpoint
        };
        db.Set<SubtitleAsset>().Add(sub);
        await db.SaveChangesAsync(ct);

        return sub;
    }
}