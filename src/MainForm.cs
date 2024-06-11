using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;

class MainForm : Form
{
    [DllImport("Shell32", CharSet = CharSet.Auto, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    static extern int ShellMessageBox(IntPtr hAppInst = default, IntPtr hWnd = default, string lpcText = default, string lpcTitle = "Error", int fuStyle = 0x00000010);

    [DllImport("Wininet")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    static extern bool InternetGetConnectedState(int lpdwFlags = 0, int dwReserved = 0);

    internal MainForm()
    {
        Application.ThreadException += (sender, e) =>
        {
            var exception = e.Exception;
            while (exception.InnerException != null) exception = exception.InnerException;
            ShellMessageBox(hWnd: Handle, lpcText: exception.Message);
            Close();
        };

        Font = new("MS Shell Dlg 2", 8);
        Text = "Dungeons Updater";
        MaximizeBox = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        ClientSize = LogicalToDeviceUnits(new System.Drawing.Size(380, 115));
        CenterToScreen();

        Label label1 = new()
        {
            Text = "Updating Minecraft Dungeons...",
            AutoSize = true,
            Location = new(LogicalToDeviceUnits(9), LogicalToDeviceUnits(23)),
            Margin = default
        };
        Controls.Add(label1);

        ProgressBar progressBar = new()
        {
            Width = LogicalToDeviceUnits(359),
            Height = LogicalToDeviceUnits(23),
            Location = new(LogicalToDeviceUnits(11), LogicalToDeviceUnits(46)),
            Margin = default,
            MarqueeAnimationSpeed = 30,
            Style = ProgressBarStyle.Marquee
        };
        Controls.Add(progressBar);

        Label label2 = new()
        {
            Text = "Checking...",
            AutoSize = true,
            Location = new(label1.Location.X, LogicalToDeviceUnits(80)),
            Margin = default
        };
        Controls.Add(label2);

        Button button = new()
        {
            Text = "Cancel",
            Width = LogicalToDeviceUnits(75),
            Height = LogicalToDeviceUnits(23),
            Location = new(LogicalToDeviceUnits(294), LogicalToDeviceUnits(81)),
            Margin = default
        };
        button.Click += (sender, e) => Close();
        Controls.Add(button);

        using WebClient webClient = new();
        webClient.DownloadProgressChanged += (sender, e) =>
        {
            label2.Text = $"Downloading... ({(float)e.BytesReceived / 1024 / 1024:0.0} MB of {(float)e.TotalBytesToReceive / 1024 / 1024:0.0} MB)";
            progressBar.Value = e.ProgressPercentage;
        };
        webClient.DownloadFileCompleted += (sender, e) =>
        {
            label2.Text = "Downloading...";
            progressBar.Value = 0;
        };

        Shown += async (sender, e) =>
        {
            if (InternetGetConnectedState())
            {
                var files = await Product.GetFilesAsync();
                if (files.Any())
                {
                    progressBar.Style = ProgressBarStyle.Blocks;
                    label2.Text = "Downloading...";

                    var count = files.Count();
                    for (int i = 0; i < count; i++)
                    {
                        var file = files.ElementAt(i);
                        label1.Text = $"Updating Minecraft Dungeons... ({i + 1} of {count})";
                        await webClient.DownloadFileTaskAsync(file.Url, file.Path);
                    }
                }
            }

            Process.Start(new ProcessStartInfo { FileName = @"Content\Dungeons.exe", UseShellExecute = false }).Dispose();
            Close();
        };

    }
}