using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using MoviePortal.Models;
using MoviePortal.Models.Common;
using MoviePortal.Models.Entities;
using MoviePortal.Models.ValueObject;
using MoviePortal.Services;

namespace MoviePortal.Components.Pages;

public partial class Subtitles : ComponentBase
{
  [Parameter] public int MovieId { get; set; }
  [Parameter] public int? EpisodeId { get; set; }
  [Parameter] public string? Lang { get; set; }

  private Movie? _movie;
  private Episode? _episode;

  private List<SubtitleAsset> _subs = new();
  private VideoAsset? _master;
  private bool _hasMaster;

  private string _lang = "vi";
  private string _label = "Tiếng Việt";

  private string _langAttach = "vi";
  private string _labelAttach = "Tiếng Việt";
  private string _vttText = "";

  private bool _isBusy;
  private string _busyText = "";
  private string? _error;

  private int _rawKey = 0, _attachKey = 0;
  
  protected override async Task OnInitializedAsync()
  {
    await LoadAsync();
  }

  private async Task LoadAsync()
  {
    _error = null;
    _movie = await Db.Movies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == MovieId);
    if (_movie is null) { _error = "Không tìm thấy phim."; return; }

    if (EpisodeId is int eid)
    {
      _episode = await Db.Episodes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == eid && x.MovieId == MovieId);
      if (_episode is null) { _error = "Không tìm thấy tập."; return; }
    }

    var q = Db.Set<SubtitleAsset>().AsNoTracking().Where(x => x.MovieId == MovieId);
    if (EpisodeId is not null) q = q.Where(x => x.EpisodeId == EpisodeId); else q = q.Where(x => x.EpisodeId == null);
    _subs = await q.OrderByDescending(x => x.Id).ToListAsync();

    var vm = Db.VideoAssets.AsNoTracking()
      .Where(v => v.MovieId == MovieId && v.Kind == VideoKind.Master);
    if (EpisodeId is not null) vm = vm.Where(v => v.EpisodeId == EpisodeId); else vm = vm.Where(v => v.EpisodeId == null);

    _master = await vm.OrderByDescending(v => v.Id).FirstOrDefaultAsync();
    _hasMaster = _master is not null;

    // set mặc định từ URL nếu có
    if (!string.IsNullOrWhiteSpace(Lang))
    {
      _lang = _langAttach = Lang!;
      _label = _labelAttach = ToLabelGuess(Lang!);
    }
  }

  private static string ToLabelGuess(string lang)
    => lang switch { "vi" => "Tiếng Việt", "en" => "English", "ja" => "日本語", "ko" => "한국어", "zh" => "中文", _ => lang };

  private string BuildPublicUrl(AssetBase a)
  {
    var endpoint = (a.Endpoint ?? "").Trim().TrimEnd('/');
    
    if (string.IsNullOrEmpty(endpoint))
      return $"/{a.Bucket}/{a.ObjectKey}";

    // Nếu thiếu scheme thì thêm mặc định http://
    if (!endpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
        !endpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
    {
      endpoint = "http://" + endpoint.TrimStart('/');
    }

    return $"{endpoint}/{a.Bucket}/{a.ObjectKey}";
  }

  private async Task BusyRun(string text, Func<Task> work)
  {
    _error = null; _busyText = text; _isBusy = true; StateHasChanged();
    try { await work(); }
    catch (Exception ex) { _error = ex.Message; }
    finally { _isBusy = false; _busyText = ""; StateHasChanged(); }
  }

  private async Task UploadRawVtt(InputFileChangeEventArgs args)
  {
    if (args.FileCount == 0) return;
    var f = args.File;
    if (!Path.GetExtension(f.Name).Equals(".vtt", StringComparison.OrdinalIgnoreCase))
    {
      _error = "Vui lòng chọn file .vtt"; return;
    }

    await BusyRun("Đang upload phụ đề (raw VTT)…", async () =>
    {
      var epNo = _episode?.Number;
      await Transcoder.UploadSubtitleVttAsync(MovieId, EpisodeId, epNo, f, _lang, _label);
      await LoadAsync();
    });

    _rawKey++;
  }

  private async Task AttachFromText()
  {
    if (string.IsNullOrWhiteSpace(_vttText)) { _error = "Chưa có nội dung VTT."; return; }
    if (!_hasMaster) { _error = "Chưa có master.m3u8 để gắn phụ đề."; return; }

    await BusyRun("Đang segment VTT & gắn vào master…", async () =>
    {
      await Transcoder.TranscodeSubtitleTextToHlsAndAttachAsync(MovieId, EpisodeId, _vttText, _langAttach, _labelAttach, 6);
      _vttText = "";
      await LoadAsync();
    });
  }

  private async Task AttachFromFile(InputFileChangeEventArgs args)
  {
    if (args.FileCount == 0) return;
    var f = args.File;
    if (!Path.GetExtension(f.Name).Equals(".vtt", StringComparison.OrdinalIgnoreCase))
    {
      _error = "Vui lòng chọn file .vtt"; return;
    }

    using var sr = new StreamReader(f.OpenReadStream(100 * 1024 * 1024));
    var text = await sr.ReadToEndAsync();

    await BusyRun("Đang segment VTT & gắn vào master…", async () =>
    {
      await Transcoder.TranscodeSubtitleTextToHlsAndAttachAsync(MovieId, EpisodeId, text, _langAttach, _labelAttach, 6);
      await LoadAsync();
    });

    _attachKey++;
  }

  private async Task DeleteSubtitle(SubtitleAsset s)
  {
    await BusyRun("Đang xoá phụ đề…", async () =>
    {
      // Xoá object chính (index.m3u8 hoặc file .vtt)
      try
      {
        // Thử xoá file chính
        await new MinioObjectStorage(Opts).DeleteAsync(s.Bucket, s.ObjectKey);
      }
      catch { /* ignore */ }

      Db.Remove(s);
      await Db.SaveChangesAsync();
      await LoadAsync();
    });
  }
}