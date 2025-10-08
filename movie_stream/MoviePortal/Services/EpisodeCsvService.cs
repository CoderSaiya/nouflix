using System.Globalization;
using Microsoft.EntityFrameworkCore;
using MoviePortal.Data;
using MoviePortal.Models;
using MoviePortal.Models.DTOs;
using MoviePortal.Models.Entities;
using MoviePortal.Models.ValueObject;
using MoviePortal.Models.Views;

namespace MoviePortal.Services;

public class EpisodeCsvService(AppDbContext db)
{
    public async Task<List<EpisodeCsvPreviewRow>> BuildPreviewAsync(string csvText)
    {
        var rows = ParseCsv(csvText);

        // Resolve movies in batch
        var ids = rows.Where(r => r.MovieId != null).Select(r => r.MovieId!.Value).Distinct().ToList();
        var slugs = rows.Where(r => !string.IsNullOrWhiteSpace(r.MovieSlug)).Select(r => r.MovieSlug!.Trim()).Distinct().ToList();
        var titles = rows.Where(r => !string.IsNullOrWhiteSpace(r.MovieTitle)).Select(r => r.MovieTitle!.Trim()).Distinct().ToList();

        var byId = ids.Count   == 0 ? new Dictionary<int, Movie>()    : await db.Movies.Where(m => ids.Contains(m.Id)).ToDictionaryAsync(m => m.Id);
        var bySlug = slugs.Count == 0 ? new Dictionary<string, Movie>() : await db.Movies.Where(m => slugs.Contains(m.Slug)).ToDictionaryAsync(m => m.Slug);
        var byTitle = titles.Count== 0 ? new Dictionary<string, Movie>() : await db.Movies.Where(m => titles.Contains(m.Title)).ToDictionaryAsync(m => m.Title);

        // Movie set to preload episodes
        var movieIdSet = new HashSet<int>();
        foreach (var r in rows)
        {
            var mid = ResolveMovieId(r, byId, bySlug, byTitle);
            if (mid.HasValue) movieIdSet.Add(mid.Value);
        }

        // Existing episode numbers per movie
        var existsMap = new Dictionary<int, HashSet<int>>();
        foreach (var mid in movieIdSet)
        {
            var nums = await db.Episodes.AsNoTracking().Where(e => e.MovieId == mid).Select(e => e.Number).ToListAsync();
            existsMap[mid] = new HashSet<int>(nums);
        }

        var preview = new List<EpisodeCsvPreviewRow>();
        var dupChecker = new HashSet<(int movieId, int number)>();
        int rowIndex = 1;

        foreach (var r in rows)
        {
            var pr = new EpisodeCsvPreviewRow { RowNumber = rowIndex++ };

            var movieId = ResolveMovieId(r, byId, bySlug, byTitle);
            if (!movieId.HasValue)
            {
                pr.Error = "Không xác định được phim (MovieId/MovieSlug/MovieTitle).";
                preview.Add(pr);
                continue;
            }

            pr.MovieId = movieId.Value;
            var movie = byId.ContainsKey(pr.MovieId) ? byId[pr.MovieId]
                      : bySlug.Values.FirstOrDefault(x => x.Id == pr.MovieId)
                    ?? byTitle.Values.First(x => x.Id == pr.MovieId);
            pr.MovieDisplay = $"{movie.Title} (#{movie.Id})";

            pr.Number = r.Number ?? 0; // force int
            pr.Title = r.Title;
            pr.Synopsis = r.Synopsis;
            pr.DurationMinutes = r.DurationMinutes;
            pr.ReleaseDate = r.ReleaseDate;
            pr.IsVipOnly = r.IsVipOnly;
            pr.SeasonNumber = r.SeasonNumber;

            pr.Status = r.Status;
            pr.StatusText = r.Status?.ToString() ?? "";
            pr.Quality = r.Quality;
            pr.QualityText = r.Quality?.ToString() ?? "";

            if (pr.Number <= 0) pr.Error = AppendErr(pr.Error, "Số tập (Number) phải > 0.");
            if (string.IsNullOrWhiteSpace(pr.Title)) pr.Error = AppendErr(pr.Error, "Thiếu Title.");

            var dupKey = (pr.MovieId, pr.Number);
            if (!dupChecker.Add(dupKey))
                pr.Error = AppendErr(pr.Error, "Trùng (MovieId,Number) ngay trong file.");

            if (existsMap.TryGetValue(pr.MovieId, out var set))
                pr.Exists = set.Contains(pr.Number);

            pr.IsValid = string.IsNullOrEmpty(pr.Error);
            preview.Add(pr);
        }

        return preview;
    }

    public async Task<EpisodeCsvImportResult> ImportAsync(IEnumerable<EpisodeCsvPreviewRow> preview, bool overwrite, bool autoCreateSeason)
    {
        var created = 0; var updated = 0; var skipped = 0; var failed = 0;
        
        var seasonsCache = new Dictionary<(int movieId, int number), int>();

        foreach (var row in preview.Where(r => r.IsValid))
        {
            try
            {
                var ep = await db.Episodes.FirstOrDefaultAsync(e => e.MovieId == row.MovieId && e.Number == row.Number);

                if (ep is not null && !overwrite) { skipped++; continue; }

                if (ep is null)
                {
                    ep = new Episode { MovieId = row.MovieId, Number = row.Number };
                    await db.Episodes.AddAsync(ep);
                    created++;
                }
                else
                {
                    updated++;
                }

                ep.Title = row.Title ?? ep.Title;
                ep.Synopsis = row.Synopsis ?? ep.Synopsis;
                ep.Duration = row.DurationMinutes.HasValue ? TimeSpan.FromMinutes(row.DurationMinutes.Value) : ep.Duration;
                ep.ReleaseDate = row.ReleaseDate;
                ep.IsVipOnly = row.IsVipOnly ?? ep.IsVipOnly;
                ep.Status = row.Status ?? ep.Status;
                ep.Quality = row.Quality ?? ep.Quality;

                if (row.SeasonNumber is not null)
                {
                    var key = (row.MovieId, row.SeasonNumber.Value);
                    if (!seasonsCache.TryGetValue(key, out var seasonId))
                    {
                        var season = await db.Seasons.FirstOrDefaultAsync(s => s.MovieId == row.MovieId && s.Number == row.SeasonNumber.Value);
                        if (season is null && autoCreateSeason)
                        {
                            season = new Season { MovieId = row.MovieId, Number = row.SeasonNumber.Value, Title = $"Season {row.SeasonNumber.Value}" };
                            await db.Seasons.AddAsync(season);
                            await db.SaveChangesAsync();
                        }
                        if (season is not null)
                        {
                            seasonId = season.Id;
                            seasonsCache[key] = seasonId;
                        }
                    }
                    if (seasonId != 0) ep.SeasonId = seasonId;
                }
            }
            catch
            {
                failed++;
            }
        }

        await db.SaveChangesAsync();
        return new EpisodeCsvImportResult(created, updated, skipped, failed);
    }

    // CSV parsing

    private List<EpisodeCsvRawRow> ParseCsv(string text)
    {
        var lines = SplitLines(text);
        if (lines.Count == 0) return new();

        var header = ParseLine(lines[0]);
        var map = header.Select((h, i) => new { h = (h ?? "").Trim(), i })
                        .ToDictionary(x => x.h.ToLowerInvariant(), x => x.i);

        var rows = new List<EpisodeCsvRawRow>();
        for (int idx = 1; idx < lines.Count; idx++)
        {
            var fields = ParseLine(lines[idx]);
            var r = new EpisodeCsvRawRow();

            string? Get(string name)
            {
                if (!map.TryGetValue(name.ToLowerInvariant(), out var i)) return null;
                return i < fields.Count ? fields[i] : null;
            }

            r.MovieId = ToInt(Get("movieid"));
            r.MovieSlug = NullIfEmpty(Get("movieslug"));
            r.MovieTitle = NullIfEmpty(Get("movietitle"));
            r.SeasonNumber = ToInt(Get("seasonnumber"));
            r.Number = ToInt(Get("number"));
            r.Title = NullIfEmpty(Get("title"));
            r.Synopsis = Get("synopsis") ?? "";
            r.DurationMinutes = ToInt(Get("durationminutes"));
            r.ReleaseDate = ToDate(Get("releasedate"));
            r.Status = ToEnum<PublishStatus>(Get("status"));
            r.Quality = ToEnum<QualityLevel>(Get("quality"));
            r.IsVipOnly = ToBool(Get("isviponly"));

            rows.Add(r);
        }
        return rows;

        // helpers
        static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        static int? ToInt(string? s) => int.TryParse((s ?? "").Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : (int?)null;
        static DateTime? ToDate(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (DateTime.TryParseExact(s.Trim(), new[] { "yyyy-MM-dd", "yyyy/M/d", "dd/MM/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var d))
                return d.Date;
            return null;
        }
        static bool? ToBool(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            var v = s.Trim().ToLowerInvariant();
            if (v is "true" or "1" or "yes" or "y") return true;
            if (v is "false" or "0" or "no" or "n")  return false;
            return null;
        }
        static TEnum? ToEnum<TEnum>(string? s) where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            var t = s.Trim();
            if (int.TryParse(t, out var iv) && Enum.IsDefined(typeof(TEnum), iv)) return (TEnum)(object)iv;
            t = t.Replace(" ", "", StringComparison.OrdinalIgnoreCase).Replace("-", "", StringComparison.OrdinalIgnoreCase);
            foreach (var name in Enum.GetNames<TEnum>())
            {
                var norm = name.Replace(" ", "", StringComparison.OrdinalIgnoreCase).Replace("-", "", StringComparison.OrdinalIgnoreCase);
                if (string.Equals(norm, t, StringComparison.OrdinalIgnoreCase)) return Enum.Parse<TEnum>(name);
            }
            return null;
        }
        static List<string> SplitLines(string text)
        {
            var list = new List<string>();
            using var sr = new StringReader(text);
            string? line;
            while ((line = sr.ReadLine()) is not null)
                if (!string.IsNullOrWhiteSpace(line)) list.Add(line);
            return list;
        }
        static List<string?> ParseLine(string line)
        {
            var res = new List<string?>();
            var sb = new System.Text.StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                        else inQuotes = false;
                    }
                    else sb.Append(c);
                }
                else
                {
                    if (c == ',') { res.Add(sb.ToString()); sb.Clear(); }
                    else if (c == '"') inQuotes = true;
                    else sb.Append(c);
                }
            }
            res.Add(sb.ToString());
            for (int i = 0; i < res.Count; i++) res[i] = res[i]?.Trim();
            return res!;
        }
    }

    private static int? ResolveMovieId(
        EpisodeCsvRawRow r,
        IReadOnlyDictionary<int, Movie> byId,
        IReadOnlyDictionary<string, Movie> bySlug,
        IReadOnlyDictionary<string, Movie> byTitle)
    {
        if (r.MovieId is not null && byId.TryGetValue(r.MovieId.Value, out var m1)) return m1.Id;
        if (!string.IsNullOrWhiteSpace(r.MovieSlug) && bySlug.TryGetValue(r.MovieSlug.Trim(), out var m2)) return m2.Id;
        if (!string.IsNullOrWhiteSpace(r.MovieTitle) && byTitle.TryGetValue(r.MovieTitle.Trim(), out var m3)) return m3.Id;
        return null;
    }

    private static string? AppendErr(string? s, string add) => string.IsNullOrEmpty(s) ? add : $"{s} | {add}";
}