using System;
using System.IO;
using System.Threading;
using System.Globalization;

static class Program
{
    [STAThread]
    static void Main()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        using Mutex mutex = new(true, "BF2988D2-FF44-4A2C-BD63-2EC3889A29D3", out bool createdNew);
        if (!createdNew) return;
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content");
        Directory.CreateDirectory(path);
        Directory.SetCurrentDirectory(path);
        new MainWindow().ShowDialog();
    }
}