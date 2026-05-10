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
        // Alapértelmezett workers szám: a rendszer processzor/szálainak száma
        WorkersText.Text = Environment.ProcessorCount.ToString();
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
            // Először egyszálúan futtatjuk a kiinduló referenciaidőhöz
            single = await Task.Run(() => MandelbrotCalculator.CalculateSingleThreaded(options));

            // Ezután párhuzamosan futtatjuk a számítást
            int? workers = null;
            if (!string.IsNullOrWhiteSpace(WorkersText.Text))
            {
                if (int.TryParse(WorkersText.Text, out var w))
                {
                    if (w > 0) workers = w; // megadott pozitív szám
                    else workers = null; // 0 vagy negatív -> automatikus (null)
                }
                else
                {
                    MessageBox.Show(this, "Invalid workers value", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            parallel = await Task.Run(() => MandelbrotCalculator.CalculateParallel(options, degreeOfParallelism: workers));

            // A megjelenítéshez a párhuzamos eredmény keretét használjuk (a tartalom várhatóan azonos)
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
                pixels[idx + 0] = color.b; // B (kék)
                pixels[idx + 1] = color.g; // G (zöld)
                pixels[idx + 2] = color.r; // R (piros)
                pixels[idx + 3] = 255; // A (alfa)
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

        var t = iterations / (double)maxIter;
        // Alkalmazzunk egyszerű gamma/simítást, hogy az alacsony iterációs értékek ne egyetlen színre essenek
        t = Math.Pow(t, 0.5);

        // Hue érték 240 (kék) és 60 (sárga) között
        var hue = 240.0 * (1.0 - t) + 60.0 * t;
        const double sat = 1.0;
        const double val = 1.0;

        var (rf, gf, bf) = HsvToRgb(hue, sat, val);
        return ((byte)(Clamp01(rf) * 255), (byte)(Clamp01(gf) * 255), (byte)(Clamp01(bf) * 255));
    }

    private static (double r, double g, double b) HsvToRgb(double h, double s, double v)
    {
        // h in [0,360), s,v in [0,1]
        var hueNorm = (h % 360 + 360) % 360;
        var chroma = v * s;
        var secondComponent = chroma * (1 - Math.Abs(((hueNorm / 60.0) % 2) - 1));
        var match = v - chroma;

        double red, green, blue;
        if (hueNorm < 60)
        {
            red = chroma; green = secondComponent; blue = 0;
        }
        else if (hueNorm < 120)
        {
            red = secondComponent; green = chroma; blue = 0;
        }
        else if (hueNorm < 180)
        {
            red = 0; green = chroma; blue = secondComponent;
        }
        else if (hueNorm < 240)
        {
            red = 0; green = secondComponent; blue = chroma;
        }
        else if (hueNorm < 300)
        {
            red = secondComponent; green = 0; blue = chroma;
        }
        else
        {
            red = chroma; green = 0; blue = secondComponent;
        }

        return (red + match, green + match, blue + match);
    }

    private static double Clamp01(double v) => v < 0 ? 0 : (v > 1 ? 1 : v);
}
