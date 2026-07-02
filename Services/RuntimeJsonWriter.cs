using System.IO.Compression;
using System.Text.Json;

namespace PjskBundle2Parts.Services;

public static class RuntimeJsonWriter
{
    public const string Gzip = "gzip";
    public const string Json = "json";
    public const string Both = "both";

    public static bool IsValidMode(string value)
    {
        return NormalizeMode(value) is Gzip or Json or Both;
    }

    public static string NormalizeMode(string value)
    {
        return value.Trim().ToLowerInvariant();
    }

    public static string PrimaryPath(string jsonPath, string mode)
    {
        return NormalizeMode(mode) == Json ? jsonPath : GzipPath(jsonPath);
    }

    public static bool OutputsExist(string jsonPath, string mode)
    {
        return NormalizeMode(mode) switch
        {
            Json => File.Exists(jsonPath),
            Both => File.Exists(jsonPath) && File.Exists(GzipPath(jsonPath)),
            _ => File.Exists(GzipPath(jsonPath)),
        };
    }

    public static void Write<T>(string jsonPath, T value, JsonSerializerOptions options, string mode)
    {
        var normalizedMode = NormalizeMode(mode);
        var parent = Path.GetDirectoryName(jsonPath);
        if (!string.IsNullOrWhiteSpace(parent))
        {
            Directory.CreateDirectory(parent);
        }

        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, options);
        if (normalizedMode is Json or Both)
        {
            File.WriteAllBytes(jsonPath, bytes);
        }
        if (normalizedMode is Gzip or Both)
        {
            using var file = File.Create(GzipPath(jsonPath));
            using var gzip = new GZipStream(file, CompressionLevel.Optimal);
            gzip.Write(bytes);
        }
    }

    public static string GzipPath(string jsonPath)
    {
        return jsonPath + ".gz";
    }
}
