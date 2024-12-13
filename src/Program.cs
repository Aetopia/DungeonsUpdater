using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Globalization;

static partial class Program
{
    internal static readonly HttpClient Client = new() { Timeout = Timeout.InfiniteTimeSpan };

    [STAThread]
    static void Main()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        using Mutex mutex = new(true, "BF2988D2-FF44-4A2C-BD63-2EC3889A29D3", out bool createdNew); if (!createdNew) return;
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content");
        Directory.CreateDirectory(path); Directory.SetCurrentDirectory(path);
        new Window().ShowDialog();
    }
}
