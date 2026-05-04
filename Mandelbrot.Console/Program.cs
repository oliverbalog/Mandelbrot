namespace Mandelbrot.Console
{
    using System;
    using System.Text;
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // Single-threaded Mandelbrot renderer in the console
            Console.CursorVisible = false;
            try
            {
                int width = Math.Max(10, Console.WindowWidth);
                int height = Math.Max(10, Console.WindowHeight - 1);

                // Render with a reasonable default
                MandelbrotRenderer.Render(width, height, maxIter: 200);

                Console.SetCursorPosition(0, height);
                Console.CursorVisible = true;
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(intercept: true);
            }
            finally
            {
                Console.CursorVisible = true;
            }

            await Task.CompletedTask;
        }
    }

    internal static class MandelbrotRenderer
    {
        // Renders a Mandelbrot set to the console (single-threaded)
        public static void Render(int width, int height, int maxIter = 100, double centerX = -0.5, double centerY = 0.0, double scale = 3.0)
        {
            var sb = new StringBuilder(height * (width + Environment.NewLine.Length));

            // Adjust aspect ratio because console characters are taller than they are wide
            double aspect = (double)height / width * 0.5;

            double halfW = width / 2.0;
            double halfH = height / 2.0;
            double scaleX = scale / width;
            double scaleY = scale * aspect / height;

            // Simple character palette from 'empty' to 'filled'
            const string palette = " .:-=+*#%@"; // index 0 = light, last = dense
            int palLen = palette.Length;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double cr = centerX + (x - halfW) * scaleX;
                    double ci = centerY + (y - halfH) * scaleY;

                    double zr = 0.0, zi = 0.0;
                    int iter = 0;
                    while (zr * zr + zi * zi <= 4.0 && iter < maxIter)
                    {
                        double tmp = zr * zr - zi * zi + cr;
                        zi = 2.0 * zr * zi + ci;
                        zr = tmp;
                        iter++;
                    }

                    int idx = (int)( (double)iter / maxIter * (palLen - 1) );
                    if (idx < 0) idx = 0;
                    if (idx >= palLen) idx = palLen - 1;

                    // Points that did not escape (likely in the set) are rendered with the densest character
                    if (iter >= maxIter) idx = palLen - 1;

                    sb.Append(palette[idx]);
                }
                sb.AppendLine();
            }

            Console.SetCursorPosition(0, 0);
            Console.Write(sb.ToString());
        }
    }
}