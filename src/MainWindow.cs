using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Diagnostics;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Windows.Forms.Integration;

class MainWindow : Window
{
    [DllImport("Shell32", CharSet = CharSet.Auto, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    static extern int ShellMessageBox(IntPtr hAppInst = default, IntPtr hWnd = default, string lpcText = default, string lpcTitle = "Error", int fuStyle = 0x00000010);

    enum Units { B, KB, MB, GB };

    static string Format(float bytes)
    {
        int value = 0;
        while (bytes >= 1024f) { bytes /= 1024f; value++; }
        return string.Format($"{bytes:0.00} {(Units)value}");
    }

    internal MainWindow()
    {
        Dispatcher.UnhandledException += (sender, e) =>
        {
            e.Handled = true;
            var exception = e.Exception;
            while (exception.InnerException != null) exception = exception.InnerException;
            ShellMessageBox(hWnd: new WindowInteropHelper(this).Handle, lpcText: exception.Message);
            Close();
        };

        UseLayoutRounding = true;
        Icon = global::Resources.Icon;
        Title = "Dungeons Updater";
        Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
        Content = new Grid { Width = 1000, Height = 600 };
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        ResizeMode = ResizeMode.NoResize;
        SizeToContent = SizeToContent.WidthAndHeight;
        Closed += (sender, e) => Environment.Exit(0);

        WindowsFormsHost host = new()
        {
            Child = new System.Windows.Forms.WebBrowser
            {
                ScrollBarsEnabled = false,
                DocumentText = $@"<!DOCTYPE html><html><head><meta http-equiv=""X-UA-Compatible"" content=""IE=edge""></head><body style=""background-color:#1E1E1E""><div style=""width:85%;height:100%;position:absolute;left:50%;top:50%;transform:translate(-50%, -50%)"">{(global::Resources.Logo)}</div></body></html>"
            },
            IsEnabled = false
        };

        Grid.SetRow(host, 0);
        ((Grid)Content).RowDefinitions.Add(new());
        ((Grid)Content).Children.Add(host);

        Grid grid = new() { Margin = new(10, 0, 10, 10) };
        grid.RowDefinitions.Add(new());

        Grid.SetRow(grid, 1);
        ((Grid)Content).RowDefinitions.Add(new() { Height = GridLength.Auto });
        ((Grid)Content).Children.Add(grid);

        ProgressBar progressBar = new()
        {
            Height = 32,
            BorderThickness = default,
            IsIndeterminate = true,
            Foreground = new SolidColorBrush(Color.FromRgb(0, 133, 66)),
            Background = new SolidColorBrush(Color.FromRgb(14, 14, 14))
        };

        Grid.SetRow(progressBar, 0);
        grid.Children.Add(progressBar);

        TextBlock textBlock1 = new()
        {
            Text = "Connecting...",
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new(16, 0, 0, 1),
            Foreground = Brushes.White
        };

        Grid.SetRow(textBlock1, 0);
        grid.Children.Add(textBlock1);

        TextBlock textBlock2 = new()
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new(0, 0, 16, 1),
            Foreground = Brushes.White
        };

        Grid.SetRow(textBlock2, 0);
        grid.Children.Add(textBlock2);


        using WebClient client = new();
        string value = default;

        client.DownloadProgressChanged += (sender, e) =>
        {
            var text = $"Downloading {Format(e.BytesReceived)} / {value ??= Format(e.TotalBytesToReceive)}";
            Dispatcher.Invoke(() =>
            {
                textBlock1.Text = text;
                if (progressBar.Value != e.ProgressPercentage) progressBar.Value = e.ProgressPercentage;
            });
        };

        client.DownloadFileCompleted += (sender, e) =>
        {
            value = null;
            Dispatcher.Invoke(() =>
            {
                progressBar.Value = 0;
                textBlock1.Text = "Downloading...";
            });
        };

        ContentRendered += async (sender, e) => await Task.Run(() =>
        {
            var artifacts = Dungeons.GetAsync();
            IList<IArtifact> files = [];

            if (artifacts.Count != 0)
            {
                Dispatcher.Invoke(() =>
                {
                    textBlock1.Text = "Verifying...";
                    progressBar.IsIndeterminate = false;
                    progressBar.Maximum = artifacts.Count;
                });

                using SHA1 sha1 = SHA1.Create();
                for (int i = 0; i < artifacts.Count; i++)
                {
                    Dispatcher.Invoke(() => textBlock2.Text = $"{progressBar.Value = i + 1} of {artifacts.Count}");
                    if (File.Exists(artifacts[i].File))
                        using (var inputStream = File.OpenRead(artifacts[i].File))
                            if (artifacts[i].SHA1.Equals(BitConverter.ToString(sha1.ComputeHash(inputStream)).Replace("-", string.Empty), StringComparison.OrdinalIgnoreCase))
                                continue;
                    files.Add(artifacts[i]);
                }
            }

            if (files.Count != 0)
            {
                Dispatcher.Invoke(() =>
                {
                    progressBar.Value = 0;
                    progressBar.Maximum = 100;
                    textBlock1.Text = "Downloading...";
                });

                for (int i = 0; i < files.Count; i++)
                {
                    Dispatcher.Invoke(() => textBlock2.Text = $"{i + 1} of {files.Count}");
                    Directory.CreateDirectory(Path.GetDirectoryName(files[i].File));
                    client.DownloadFileTaskAsync(files[i].Url, files[i].File).Wait();
                }
            }

            Process.Start(new ProcessStartInfo { FileName = @"Content\Dungeons.exe", UseShellExecute = false }).Dispose();
            Dispatcher.Invoke(Close);
        });
    }
}