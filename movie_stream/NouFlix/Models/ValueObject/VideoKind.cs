namespace NouFlix.Models.ValueObject;

public enum VideoKind
{
    Primary = 1,
    Trailer,
    Extra,
    Master  = 10, // HLS master.m3u8
    Variant = 11  // HLS /{quality}/index.m3u8
}