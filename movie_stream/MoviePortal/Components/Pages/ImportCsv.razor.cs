using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MoviePortal.Models.Views;
using MoviePortal.Services;

namespace MoviePortal.Components.Pages;

public partial class ImportCsv : ComponentBase
{
    [Inject] private EpisodeCsvService CsvService { get; set; } = null!;
    [Inject] private IJSRuntime Js { get; set; } = null!;

    // UI state
    private IBrowserFile? _file;
    private string? _fileName;
    private string _fileSizeStr = "";
    private bool _overwrite = false;
    private bool _autoCreateSeason = true;

    private List<EpisodeCsvPreviewRow>? _preview;
    private string? _result;

    private bool CanParse  => _file is not null;
    private bool CanImport => _preview is not null && _preview.Any(x => x.IsValid) && _file is not null;

    protected void OnFileSelected(InputFileChangeEventArgs e)
    {
        _preview = null;
        _result = null;

        _file = e.File;
        _fileName = _file?.Name;
        if (_file is not null)
            _fileSizeStr = $"{_file.Size / 1024.0:0.#} KB";
    }

    private async Task ParseAndPreview()
    {
        if (_file is null) return;
        await using var stream = _file.OpenReadStream(long.MaxValue);
        using var reader = new StreamReader(stream);
        var text = await reader.ReadToEndAsync();

        _preview = await CsvService.BuildPreviewAsync(text);
    }

    private async Task ImportAsync()
    {
        if (_preview is null) return;
        var ok = await Js.InvokeAsync<bool>("confirm", "Xác nhận nhập dữ liệu theo bảng xem trước?");
        if (!ok) return;

        var result = await CsvService.ImportAsync(_preview, _overwrite, _autoCreateSeason);
        _result = $"Hoàn tất. Tạo mới: {result.Created}, Cập nhật: {result.Updated}, Bỏ qua: {result.Skipped}, Lỗi: {result.Failed}.";
    }
}