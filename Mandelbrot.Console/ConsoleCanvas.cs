namespace Mandelbrot.Console;

internal static class ConsoleCanvas
{
    private const int MinimumWidth = 10;
    private const int MinimumHeight = 10;

    public static ConsoleCanvasSize GetDrawableSize()
    {
        var width = Math.Max(MinimumWidth, System.Console.WindowWidth);
        var height = Math.Max(MinimumHeight, System.Console.WindowHeight - 1);

        return new ConsoleCanvasSize(width, height);
    }
}
