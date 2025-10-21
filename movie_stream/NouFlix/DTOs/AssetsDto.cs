using Microsoft.AspNetCore.Components.Forms;
using NouFlix.Models.ValueObject;

namespace NouFlix.DTOs;

public class AssetsDto
{
    public record VideoAssetRes(
        int Id,
        int? MovieId,
        int? EpisodeId,
        VideoKind Kind,
        QualityLevel Quality,
        string Bucket,
        string ObjectKey);

    public record ImageAssetRes(
        int Id,
        string Alt,
        string Endpoint,
        string Bucket,
        string ObjectKey,
        string Url);
    
    public record PreviewReq(string Bucket, string ObjectKey);
}