using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

file static class Resources
{
    static readonly Assembly assembly = Assembly.GetExecutingAssembly();

    internal static readonly string Dungeons = ToString("Dungeons.svg");

    internal static readonly ImageSource Icon = ToImageSource(".ico");

    static ImageSource ToImageSource(string name)
    {
        using var stream = assembly.GetManifestResourceStream(name);
        return BitmapFrame.Create(stream);
    }

    static string ToString(string name)
    {
        using var stream = assembly.GetManifestResourceStream(name);
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}

class MainWindow : Window
{
    [DllImport("Shell32", CharSet = CharSet.Auto, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    static extern int ShellMessageBox(IntPtr hAppInst = default, IntPtr hWnd = default, string lpcText = default, string lpcTitle = "Error", int fuStyle = 0x00000010);

    static readonly IEnumerable<string> Units = ["B", "KB", "MB", "GB"];

    static string Format(float bytes)
    {
        int index = 0;
        while (bytes >= 1024) { bytes /= 1024; ++index; }
        return string.Format($"{bytes:0.00} {Units.ElementAt(index)}");
    }

    internal MainWindow()
    {
        Application.Current.DispatcherUnhandledException += (sender, e) =>
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
        SizeToContent = SizeToContent.WidthAndHeight;
        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
        Content = new Grid { Width = 1000, Height = 600 };
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        ResizeMode = ResizeMode.NoResize;

        WindowsFormsHost host = new()
        {
            Child = new System.Windows.Forms.WebBrowser()
            {
                ScrollBarsEnabled = false,
                DocumentText = $@"<head><meta http-equiv=""X-UA-Compatible"" content=""IE=9""/></head><body style=""background-color: #1E1E1E""><div style=""width:85%;height:100%;position:absolute;left:50%;top:50%;transform: translate(-50%, -50%)"">{(global::Resources.Dungeons)}</div></body>"
            },
            IsEnabled = false
        };

        Grid.SetRow(host, 0);
        ((Grid)Content).RowDefinitions.Add(new());
        ((Grid)Content).Children.Add(host);

        Grid grid = new() { Margin = new(10, 0, 10, 10), };
        grid.RowDefinitions.Add(new());

        Grid.SetRow(grid, 1);
        ((Grid)Content).RowDefinitions.Add(new() { Height = GridLength.Auto });
        ((Grid)Content).Children.Add(grid);

        ProgressBar progressBar = new()
        {
            Height = 32,
            BorderThickness = default,
            IsIndeterminate = true,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#008542")),
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0E0E0E"))
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
            if (progressBar.Value != e.ProgressPercentage)
            {
                textBlock1.Text = $"Downloading {Format(e.BytesReceived)} / {value ??= Format(e.TotalBytesToReceive)}";
                progressBar.Value = e.ProgressPercentage;
            }
        };

        client.DownloadFileCompleted += (sender, e) =>
            {
                value = null;
                progressBar.Value = 0;
                textBlock1.Text = "Downloading...";
            };

        ContentRendered += async (sender, e) =>
        {
            var artifacts = await Dungeons.GetAsync();
            IList<IArtifact> files = [];

            if (artifacts.Count != 0)
            {
                textBlock1.Text = "Verifying...";
                progressBar.IsIndeterminate = false;
                progressBar.Maximum = artifacts.Count;

                await Task.Run(() =>
                {
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
                });
            }
            if (files.Count != 0)
            {
                progressBar.Value = 0;
                progressBar.Maximum = 100;
                textBlock1.Text = "Downloading...";

                for (int i = 0; i < files.Count; i++)
                {
                    textBlock2.Text = $"{i + 1} of {files.Count}";
                    Directory.CreateDirectory(Path.GetDirectoryName(files[i].File));
                    await client.DownloadFileTaskAsync(files[i].Url, files[i].File);
                }
            }

            Process.Start(new ProcessStartInfo { FileName = @"Content\Dungeons.exe", UseShellExecute = false }).Dispose();
            Close();
        };
    }
}