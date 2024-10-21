using System;
using System.Windows;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Diagnostics;

class Window : System.Windows.Window
{
    [DllImport("Shell32", CharSet = CharSet.Auto, SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    static extern int ShellMessageBox(IntPtr hAppInst = default, IntPtr hWnd = default, string lpcText = default, string lpcTitle = "Error", int fuStyle = 0x00000010);

    enum Unit { B, KB, MB, GB }

    static string Size(double value) { var unit = (int)Math.Log(value, 1024); return $"{value / Math.Pow(1024, unit):0.00} {(Unit)unit}"; }

    internal Window()
    {
        Icon = global::Resources.GetImageSource(".ico");
        UseLayoutRounding = true;
        Title = "Dungeons Updater";
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        SizeToContent = SizeToContent.WidthAndHeight;
        Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
        var text = "Updating Dungeons...";

        Canvas canvas = new() { Width = 381, Height = 115 }; Content = canvas;

        TextBlock block1 = new() { Text = text, Foreground = Brushes.White };
        canvas.Children.Add(block1); Canvas.SetLeft(block1, 11); Canvas.SetTop(block1, 15);

        TextBlock block2 = new() { Text = "Preparing...", Foreground = Brushes.White };
        canvas.Children.Add(block2); Canvas.SetLeft(block2, 11); Canvas.SetTop(block2, 84);

        ProgressBar bar = new()
        {
            Width = 359,
            Height = 23,
            BorderThickness = default,
            IsIndeterminate = true,
            Foreground = new SolidColorBrush(Color.FromRgb(0, 133, 66)),
            Background = new SolidColorBrush(Color.FromRgb(14, 14, 14))
        };
        canvas.Children.Add(bar); Canvas.SetLeft(bar, 11); Canvas.SetTop(bar, 46);

        Dispatcher.UnhandledException += (_, e) =>
        {
            e.Handled = true; var exception = e.Exception;
            while (exception.InnerException is not null) exception = exception.InnerException;
            ShellMessageBox(hWnd: new WindowInteropHelper(this).Handle, lpcText: exception.Message);
            Close();
        };

        ContentRendered += async (_, _) => await Task.Run(() =>
        {
            var jobs = Client.Get();
            Dispatcher.Invoke(() => { bar.Maximum = jobs.Count; bar.IsIndeterminate = false; });

            jobs = jobs.Verify((current, total) => Dispatcher.Invoke(() => block2.Text = $"Preparing... {bar.Value = current} / {total}"));
            Dispatcher.Invoke(() => { bar.Value = 0; bar.Maximum = 100; block2.Text = "Preparing..."; bar.IsIndeterminate = true; });

            string value = default;
            jobs.Download((progress, current, total) => Dispatcher.Invoke(() =>
            {
                if (bar.Value != progress)
                {
                    if (bar.IsIndeterminate) bar.IsIndeterminate = false;
                    block2.Text = $"Preparing... {Size(current)} / {value ??= Size(total)}";
                    bar.Value = progress;
                }
            }));
            Process.Start(new ProcessStartInfo { FileName = @"Dungeons.exe", UseShellExecute = false }).Dispose();
            Dispatcher.Invoke(Close);
        });
    }
}