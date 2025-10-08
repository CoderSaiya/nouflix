namespace MoviePortal.Models.ValueObject;

public enum VideoKind
{
    Primary = 0, // mp4
    Master  = 10, // HLS master.m3u8
    Variant = 11  // HLS /{quality}/index.m3u8
}