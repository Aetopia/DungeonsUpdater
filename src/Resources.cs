using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

static class Resources
{
    static readonly Assembly assembly = Assembly.GetExecutingAssembly();

    internal static ImageSource GetImageSource(string name)
    {
        using var stream = assembly.GetManifestResourceStream(name);
        return BitmapFrame.Create(stream);
    }

    internal static string GetString(string name)
    {
        using var _ = assembly.GetManifestResourceStream(name);
        using GZipStream stream = new(_, CompressionMode.Decompress);
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}