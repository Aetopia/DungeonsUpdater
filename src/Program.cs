using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Windows.Forms;

static class Program
{
    static void Main()
    {
        using Mutex mutex = new(true, "BF2988D2-FF44-4A2C-BD63-2EC3889A29D3", out bool createdNew);
        if (!createdNew) return;
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

        ((NameValueCollection)ConfigurationManager.GetSection("System.Windows.Forms.ApplicationConfigurationSection"))["DpiAwareness"] = "PerMonitorV2";
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}