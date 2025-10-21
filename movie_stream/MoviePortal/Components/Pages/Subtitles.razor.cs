using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MoviePortal.Api;

namespace MoviePortal.Components.Pages;

public partial class Subtitles : ComponentBase
{
  [Parameter] public int MovieId { get; set; }
  [Parameter] public int? EpisodeId { get; set; }
  [Parameter] public string? Lang { get; set; }
  [Inject] public SubtitlesApi Api { get; set; } = null!;

  private string _lang = "vi";
  private string _label = "Tiếng Việt";

  private bool _isBusy;
  private string _busyText = "";
  private string? _error;
  private string _uploadedUrl = "";

  protected override void OnInitialized()
  {
    if (!string.IsNullOrWhiteSpace(Lang))
    {
      _lang = Lang!;
      _label = Lang switch { "vi" => "Tiếng Việt", "en" => "English", "ja" => "日本語", "ko" => "한국어", "zh" => "中文", _ => Lang! };
    }
  }

  protected async Task UploadRawVtt(InputFileChangeEventArgs e)
  {
    if (e.FileCount == 0) return;
    var file = e.File;
    if (!Path.GetExtension(file.Name).Equals(".vtt", StringComparison.OrdinalIgnoreCase))
    {
      _error = "Vui lòng chọn file .vtt"; return;
    }

    _error = null; _uploadedUrl = ""; _isBusy = true; _busyText = "Đang upload phụ đề…"; StateHasChanged();

    try
    {
      var res = await Api.UploadRawVttAsync(MovieId, EpisodeId, _lang, _label, file);
      _uploadedUrl = res?.PublicUrl ?? "";
    }
    catch (Exception ex)
    {
      _error = ex.Message;
    }
    finally
    {
      _isBusy = false; _busyText = ""; StateHasChanged();
    }
  }
}