namespace Mandelbrot.Console;

internal static class LoggingPaths
{
    public static string GetDefaultLogFilePath()
    {
        var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        var fileName = $"mandelbrot-{DateTime.Now:yyyyMMdd}.log";

        return Path.Combine(logDirectory, fileName);
    }
}
