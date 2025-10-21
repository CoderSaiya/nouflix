// using Microsoft.AspNetCore.Components;
// using Microsoft.AspNetCore.Components.Forms;
// using Microsoft.JSInterop;
// using MoviePortal.Models.DTOs;
// using MoviePortal.Models.Entities;
// using MoviePortal.Models.ValueObject;
// using MoviePortal.Services;
//
// namespace MoviePortal.Components.Pages;
//
// public partial class StoragePage : ComponentBase
// {
//     [Inject] private StorageService Storage { get; set; } = null!;
//     [Inject] private IConfiguration Cfg { get; set; } = null!;
//     [Inject] private IJSRuntime Js { get; set; } = null!;
//
//     // Tabs
//     protected string Tab { get; private set; } = "images";
//     protected void SetTab(string t) { Tab = t; StateHasChanged(); }
//
//     // Images
//     protected string? ImgMovieId;
//     protected string? ImgEpisodeId;
//     protected string? ImgKind;
//     protected string? ImgQ;
//     protected readonly string[] ImgKinds = Enum.GetNames<ImageKind>();
//     protected List<ImageAsset> Images { get; private set; } = new();
//
//     protected async Task SearchImagesAsync()
//     {
//         int? mid = int.TryParse(ImgMovieId, out var m) ? m : null;
//         int? eid = int.TryParse(ImgEpisodeId, out var e) ? e : null;
//         ImageKind? kind = !string.IsNullOrWhiteSpace(ImgKind) && Enum.TryParse<ImageKind>(ImgKind, out var k) ? k : null;
//
//         Images = await Storage.FindImagesAsync(new ImageFilter(
//             MovieId: mid, EpisodeId: eid, Kind: kind, Q: ImgQ, Take: 200
//         ));
//     }
//
//     // protected async Task SaveAltAsync(ImageAsset img)
//     // {
//     //     await Storage.UpdateImageAltAsync(img.Id, img.Alt ?? "");
//     //     // không cần reload toàn bộ
//     // }
//
//     protected async Task DeleteImageAsync(int id)
//     {
//         var ok = await Js.InvokeAsync<bool>("confirm", "Xoá ảnh này?");
//         if (!ok) return;
//
//         var img = Images.FirstOrDefault(x => x.Id == id);
//         if (img is null) return;
//
//         await Storage.DeleteImageAsync(img.Id);
//         await SearchImagesAsync();
//     }
//
//     // Videos
//     protected string? VidMovieId;
//     protected string? VidEpisodeId;
//     protected string? VidKind;
//     protected string? VidQuality;
//     protected string? VidQ;
//     protected readonly string[] VidKinds = Enum.GetNames<VideoKind>();
//     protected readonly string[] Qualities = Enum.GetNames<QualityLevel>();
//     protected readonly string[] Statuses  = Enum.GetNames<PublishStatus>();
//     protected List<VideoAsset> Videos { get; private set; } = new();
//     protected string? PreviewUrl;
//
//     protected async Task SearchVideosAsync()
//     {
//         int? mid = int.TryParse(VidMovieId, out var m) ? m : null;
//         int? eid = int.TryParse(VidEpisodeId, out var e) ? e : null;
//         VideoKind? vk = !string.IsNullOrWhiteSpace(VidKind) && Enum.TryParse<VideoKind>(VidKind, out var k) ? k : null;
//         QualityLevel? ql = !string.IsNullOrWhiteSpace(VidQuality) && Enum.TryParse<QualityLevel>(VidQuality, out var q) ? q : null;
//
//         Videos = await Storage.FindVideosAsync(new VideoFilter(
//             MovieId: mid, EpisodeId: eid, Kind: vk, Quality: ql, Q: VidQ, Take: 200
//         ));
//     }
//
//     protected async Task PreviewAsync(VideoAsset v)
//     {
//         var url = await Storage.GetReadSignedUrlAsync(v.Bucket, v.ObjectKey, TimeSpan.FromMinutes(10));
//         PreviewUrl = url.ToString();
//     }
//
//     protected async Task DeleteVideoAsync(int id)
//     {
//         var ok = await Js.InvokeAsync<bool>("confirm", "Xoá video này?");
//         if (!ok) return;
//
//         var v = Videos.FirstOrDefault(x => x.Id == id);
//         if (v is null) return;
//
//         await Storage.DeleteVideoAsync(v.Id);
//         await SearchVideosAsync();
//     }
//
//     // Upload
//     protected string ImgContainer = "";
//     protected string VidContainer = "";
//     protected IBrowserFile? UpImgFile;
//     protected IBrowserFile? UpVidFile;
//
//     protected string? UpImgMovieId;
//     protected string? UpImgEpisodeId;
//     protected string UpImgKind = Enum.GetNames<ImageKind>()[0];
//     protected string UpImgAlt = "";
//     protected string? UpImgResult;
//
//     protected string? UpVidMovieId;
//     protected string? UpVidEpisodeId;
//     protected string UpVidKind = Enum.GetNames<VideoKind>()[0];
//     protected string UpVidQuality = Enum.GetNames<QualityLevel>()[0];
//     protected string UpVidLang = "vi";
//     protected string UpVidStatus = Enum.GetNames<PublishStatus>()[0];
//     protected string? UpVidResult;
//     
//     private long MaxImageBytes => Cfg.GetValue<long?>("AdminMedia:MaxImageBytes") ?? 10 * 1024 * 1024; // 10MB
//     private long MaxVideoBytes => Cfg.GetValue<long?>("AdminMedia:MaxVideoBytes") ?? 5L * 1024 * 1024 * 1024; // 5GB
//
//     protected override void OnInitialized()
//     {
//         ImgContainer = Cfg["Storage:S3:Buckets:Images"] ?? ImgContainer;
//         VidContainer = Cfg["Storage:S3:Buckets:Videos"] ?? VidContainer;
//     }
//
//     protected void OnImgSelected(InputFileChangeEventArgs e) => UpImgFile = e.File;
//     protected void OnVidSelected(InputFileChangeEventArgs e) => UpVidFile = e.File;
//
//     protected async Task UploadImageAsync()
//     {
//         try
//         {
//             if (UpImgFile is null || string.IsNullOrWhiteSpace(ImgContainer))
//             {
//                 UpImgResult = "Thiếu file hoặc bucket ảnh.";
//                 return;
//             }
//             if (UpImgFile.Size <= 0) { UpImgResult = "File rỗng."; return; }
//             if (UpImgFile.Size > MaxImageBytes) { UpImgResult = $"Ảnh quá lớn (>{FormatBytes(MaxImageBytes)})."; return; }
//
//             int? mid = int.TryParse(UpImgMovieId, out var m) ? m : null;
//             int? eid = int.TryParse(UpImgEpisodeId, out var e) ? e : null;
//             
//             if (!Enum.TryParse<ImageKind>(UpImgKind, true, out var kind)) kind = ImageKind.Poster;
//
//             await using var s = UpImgFile.OpenReadStream(MaxImageBytes);
//             var img = await Storage.UploadImageAsync(
//                 movieId: mid ?? -1,
//                 file: s,
//                 fileName: UpImgFile.Name,
//                 contentType: UpImgFile.ContentType,
//                 kind: kind,
//                 alt: string.IsNullOrWhiteSpace(UpImgAlt) ? null : UpImgAlt
//             );
//
//             UpImgResult = $"Đã upload ảnh #{img.Id}: {img.Bucket}/{img.ObjectKey}";
//             if (Tab == "images") await SearchImagesAsync();
//         }
//         catch (Exception ex)
//         {
//             UpImgResult = $"Lỗi upload ảnh: {ex.Message}";
//         }
//     }
//
//     protected async Task UploadVideoAsync()
//     {
//         if (UpVidFile is null || string.IsNullOrWhiteSpace(VidContainer)) return;
//
//         int? mid = int.TryParse(UpVidMovieId, out var m) ? m : null;
//         int? eid = int.TryParse(UpVidEpisodeId, out var e) ? e : null;
//         
//         Enum.TryParse<VideoKind>(UpVidKind, out var kind);
//         Enum.TryParse<QualityLevel>(UpVidQuality, out var ql);
//
//         await using var s = UpVidFile.OpenReadStream(MaxVideoBytes);
//         var vid = await Storage.UploadVideoAsync(
//             movieId: mid ?? -1,
//             file: s,
//             fileName: UpVidFile.Name,
//             contentType: UpVidFile.ContentType,
//             kind: kind,
//             quality: ql,
//             language: UpVidLang
//         );
//
//         UpVidResult = $"Đã upload video #{vid.Id}: {vid.Bucket}/{vid.ObjectKey}";
//         if (Tab == "videos") await SearchVideosAsync();
//     }
//
//     // Utils
//     protected static string FormatBytes(long bytes)
//     {
//         string[] sizes = { "B","KB","MB","GB","TB" };
//         if (bytes <= 0) return "0 B";
//         var i = (int)Math.Floor(Math.Log(bytes, 1024));
//         var v = bytes / Math.Pow(1024, i);
//         return $"{v:0.##} {sizes[i]}";
//     }
// }