using Mandelbrot.Application;

namespace Mandelbrot.Console;

internal static class Program
{
    private static void Main()
    {
        var cursorWasVisible = System.Console.CursorVisible;

        try
        {
            System.Console.CursorVisible = false;

            var size = ConsoleCanvas.GetDrawableSize();
            var options = new MandelbrotOptions(size.Width, size.Height);
            var frame = MandelbrotCalculator.Calculate(options);

            var renderer = new ConsoleMandelbrotRenderer(new CharacterPalette(" .:-=+*#%@"));
            renderer.Render(frame, options.MaxIterations);

            System.Console.SetCursorPosition(0, size.Height);
            System.Console.CursorVisible = cursorWasVisible;
            System.Console.WriteLine("Press any key to exit...");
            System.Console.ReadKey(intercept: true);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or IOException)
        {
            System.Console.CursorVisible = cursorWasVisible;
            System.Console.Error.WriteLine($"Unable to render Mandelbrot set: {ex.Message}");
        }
        finally
        {
            System.Console.CursorVisible = cursorWasVisible;
        }
    }
}
