using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;

class MainForm : Form
{
    [DllImport("Shell32", CharSet = CharSet.Auto, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    static extern int ShellMessageBox(IntPtr hAppInst = default, IntPtr hWnd = default, string lpcText = default, string lpcTitle = "Error", int fuStyle = 0x00000010);

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
            Text = "Verifying...",
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
            var artifacts = await Dungeons.GetAsync();
            IList<IArtifact> files = [];

            if (artifacts.Count != 0)
            {
                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.Maximum = artifacts.Count;
                await Task.Run(() =>
                {
                    using SHA1 sha1 = SHA1.Create();
                    for (int i = 0; i < artifacts.Count; i++)
                    {
                        label1.Text = $"Updating Minecraft Dungeons... ({progressBar.Value = i + 1} of {artifacts.Count})";
                        if (File.Exists(artifacts[i].File))
                        {
                            using var inputStream = File.OpenRead(artifacts[i].File);
                            if (artifacts[i].SHA1.Equals(BitConverter.ToString(sha1.ComputeHash(inputStream)).Replace("-", string.Empty), StringComparison.OrdinalIgnoreCase))
                                continue;
                        }
                        files.Add(artifacts[i]);
                    }
                });
            }

            if (files.Count != 0)
            {
                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.Value = 0;
                progressBar.Maximum = 100;
                label2.Text = "Downloading...";

                for (int i = 0; i < files.Count; i++)
                {
                    label1.Text = $"Updating Minecraft Dungeons... ({i + 1} of {files.Count})";
                    Directory.CreateDirectory(Path.GetDirectoryName(files[i].File));
                    await webClient.DownloadFileTaskAsync(files[i].Url, files[i].File);
                }
            }

            Process.Start(new ProcessStartInfo { FileName = @"Content\Dungeons.exe", UseShellExecute = false }).Dispose();
            Close();
        };
    }
}