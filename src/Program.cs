using System;
using System.Globalization;
using System.IO;
using System.Threading;

static class Program
{
    [STAThread]
    static void Main()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        using Mutex mutex = new(true, "BF2988D2-FF44-4A2C-BD63-2EC3889A29D3", out bool createdNew);
        if (!createdNew) return;
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        new MainWindow().ShowDialog();
    }
}