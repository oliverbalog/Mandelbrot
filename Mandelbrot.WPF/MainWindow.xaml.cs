using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Mandelbrot.Application;

namespace Mandelbrot.WPF;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void RenderButton_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(WidthText.Text, out var width) || width <= 0)
        {
            MessageBox.Show(this, "Invalid width", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (!int.TryParse(HeightText.Text, out var height) || height <= 0)
        {
            MessageBox.Show(this, "Invalid height", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (!int.TryParse(IterationsText.Text, out var maxIter) || maxIter <= 0)
        {
            MessageBox.Show(this, "Invalid max iterations", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        RenderButton.IsEnabled = false;
        StatusText.Text = "Rendering...";

        var options = new MandelbrotOptions(width, height, maxIter);

        MandelbrotCalculationResult single = default;
        MandelbrotCalculationResult parallel = default;

        try
        {
            // Run single-threaded first to provide a baseline
            single = await Task.Run(() => MandelbrotCalculator.CalculateSingleThreaded(options));

            // Then parallel
            parallel = await Task.Run(() => MandelbrotCalculator.CalculateParallel(options));

            // Use the parallel frame for display (identical content expected)
            var frame = parallel.Frame;

            var bmp = CreateBitmapFromFrame(frame, options.MaxIterations);
            MandelbrotImage.Source = bmp;

            SingleThreadedTiming.Text = $"Single-threaded: {single.Elapsed.TotalMilliseconds:F2} ms";
            ParallelTiming.Text = $"Parallel: {parallel.Elapsed.TotalMilliseconds:F2} ms (workers={parallel.DegreeOfParallelism})";
            var speedup = parallel.Elapsed.TotalMilliseconds <= 0 ? 0 : single.Elapsed.TotalMilliseconds / parallel.Elapsed.TotalMilliseconds;
            SpeedupText.Text = $"Speedup: {speedup:F2}x";
            StatusText.Text = "Completed";
        }
        catch (Exception ex)
        {
            StatusText.Text = "Error: " + ex.Message;
            MessageBox.Show(this, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            RenderButton.IsEnabled = true;
        }
    }

    private static WriteableBitmap CreateBitmapFromFrame(MandelbrotFrame frame, int maxIterations)
    {
        var width = frame.Width;
        var height = frame.Height;

        var wb = new WriteableBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null);
        var stride = width * 4;
        var pixels = new byte[height * stride];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var p = frame.GetPoint(x, y);
                var color = MapColor(p.Iterations, maxIterations, p.IsInSet);
                var idx = (y * stride) + (x * 4);
                pixels[idx + 0] = color.b; // B
                pixels[idx + 1] = color.g; // G
                pixels[idx + 2] = color.r; // R
                pixels[idx + 3] = 255; // A
            }
        }

        wb.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        return wb;
    }

    private static (byte r, byte g, byte b) MapColor(int iterations, int maxIter, bool inSet)
    {
        if (inSet)
        {
            return (0, 0, 0);
        }
        // Map iterations to a cooler hue ramp (blue -> yellow) and keep the set black.
        var t = iterations / (double)maxIter;
        // apply simple gamma/smoothing so low iteration values don't all map to the same color
        t = Math.Pow(t, 0.5);

        // hue from 240 (blue) to 60 (yellow)
        var hue = 240.0 * (1.0 - t) + 60.0 * t;
        const double sat = 1.0;
        const double val = 1.0;

        var (rf, gf, bf) = HsvToRgb(hue, sat, val);
        return ((byte)(Clamp01(rf) * 255), (byte)(Clamp01(gf) * 255), (byte)(Clamp01(bf) * 255));
    }

    private static (double r, double g, double b) HsvToRgb(double h, double s, double v)
    {
        // h in [0,360), s,v in [0,1]
        var hh = (h % 360 + 360) % 360;
        var c = v * s;
        var x = c * (1 - Math.Abs(((hh / 60.0) % 2) - 1));
        var m = v - c;

        double r1 = 0, g1 = 0, b1 = 0;
        if (hh < 60)
        {
            r1 = c; g1 = x; b1 = 0;
        }
        else if (hh < 120)
        {
            r1 = x; g1 = c; b1 = 0;
        }
        else if (hh < 180)
        {
            r1 = 0; g1 = c; b1 = x;
        }
        else if (hh < 240)
        {
            r1 = 0; g1 = x; b1 = c;
        }
        else if (hh < 300)
        {
            r1 = x; g1 = 0; b1 = c;
        }
        else
        {
            r1 = c; g1 = 0; b1 = x;
        }

        return (r1 + m, g1 + m, b1 + m);
    }

    private static double Clamp01(double v) => v < 0 ? 0 : (v > 1 ? 1 : v);
}
