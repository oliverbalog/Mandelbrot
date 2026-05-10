using Mandelbrot.Application;
using Serilog;

namespace Mandelbrot.Console;

internal static class Program
{
    private const string RunSeparator = "\n\n------------------------------------------------------------";
    private const int FrameGapWidth = 3;
    private const int TitleHeight = 1;
    private const int TimingSummaryHeight = 6;
    private const int BenchmarkWidth = 1920;
    private const int BenchmarkHeight = 1080;
    private const int BenchmarkMaxIterations = 1000;

    private static void Main()
    {
        var cursorWasVisible = System.Console.CursorVisible;
        var logger = CreateLogger();

        using var applicationOperation = TimedOperation.Start("Application run", logger);

        try
        {
            System.Console.CursorVisible = false;
            logger.Information("{RunSeparator}", RunSeparator);
            logger.Information("Application started.");

            var size = ConsoleCanvas.GetDrawableSize();
            logger.Debug("Console drawable size resolved to {Width}x{Height}.", size.Width, size.Height);

            var frameWidth = Math.Max(10, (size.Width - FrameGapWidth) / 2);
            var frameHeight = Math.Max(5, size.Height - TitleHeight - TimingSummaryHeight - 1);
            var benchmarkOptions = new MandelbrotOptions(BenchmarkWidth, BenchmarkHeight, BenchmarkMaxIterations);
            var singleThreadedBenchmarkResult = MandelbrotCalculator.CalculateSingleThreaded(benchmarkOptions, logger);
            var parallelBenchmarkResult = MandelbrotCalculator.CalculateParallel(benchmarkOptions, logger);
            LogComparison(singleThreadedBenchmarkResult, parallelBenchmarkResult, logger);

            var displayOptions = new MandelbrotOptions(frameWidth, frameHeight);
            var singleThreadedDisplayResult = MandelbrotCalculator.CalculateSingleThreaded(displayOptions, logger);
            var parallelDisplayResult = MandelbrotCalculator.CalculateParallel(displayOptions, logger);

            System.Console.Clear();

            var renderer = new ConsoleMandelbrotRenderer(new CharacterPalette(" .:-=+*#&%@"));
            TimeSpan singleThreadedRenderElapsed;
            using (var singleThreadedRenderOperation = TimedOperation.Start("Single-threaded calculation result rendering", logger))
            {
                renderer.Render(singleThreadedDisplayResult.Frame, displayOptions.MaxIterations, title: "Single-threaded");
                singleThreadedRenderElapsed = singleThreadedRenderOperation.Elapsed;
            }

            logger.Information("Rendering single threaded mandelbrot completed successfully.");

            TimeSpan parallelRenderElapsed;
            using (var parallelRenderOperation = TimedOperation.Start("Parallel calculation result rendering", logger))
            {
                renderer.Render(parallelDisplayResult.Frame, displayOptions.MaxIterations, frameWidth + FrameGapWidth, 0, "Parallel");
                parallelRenderElapsed = parallelRenderOperation.Elapsed;
            }

            logger.Information("Rendering parallel mandelbrot completed successfully.");

            WriteTimingSummary(singleThreadedBenchmarkResult, parallelBenchmarkResult, singleThreadedRenderElapsed, parallelRenderElapsed, frameHeight + TitleHeight + 1);

            System.Console.SetCursorPosition(0, Math.Min(System.Console.BufferHeight - 1, frameHeight + TitleHeight + TimingSummaryHeight + 1));
            System.Console.CursorVisible = cursorWasVisible;
            System.Console.WriteLine("Press any key to exit...");
            System.Console.ReadKey(intercept: true);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or IOException)
        {
            logger.Error(ex, "Unable to render Mandelbrot set.");
            System.Console.CursorVisible = cursorWasVisible;
            System.Console.Error.WriteLine($"Unable to render Mandelbrot set: {ex.Message}");
        }
        finally
        {
            logger.Information("Application stopped.");
            System.Console.CursorVisible = cursorWasVisible;
            Log.CloseAndFlush();
        }
    }

    private static void WriteTimingSummary(
        MandelbrotCalculationResult singleThreadedResult,
        MandelbrotCalculationResult parallelResult,
        TimeSpan singleThreadedRenderElapsed,
        TimeSpan parallelRenderElapsed,
        int top)
    {
        top = Math.Clamp(top, 0, System.Console.BufferHeight - 1);

        var savedMilliseconds = singleThreadedResult.Elapsed.TotalMilliseconds - parallelResult.Elapsed.TotalMilliseconds;
        var speedup = parallelResult.Elapsed.TotalMilliseconds <= 0
            ? 0
            : singleThreadedResult.Elapsed.TotalMilliseconds / parallelResult.Elapsed.TotalMilliseconds;

        WriteConsoleLine(0, top, "Timings");
        WriteConsoleLine(0, top + 1, $"Single-threaded calculation: {singleThreadedResult.Elapsed.TotalMilliseconds:F2} ms ({singleThreadedResult.Frame.Width}x{singleThreadedResult.Frame.Height}, {BenchmarkMaxIterations} iterations)");
        WriteConsoleLine(0, top + 2, $"Parallel calculation:        {parallelResult.Elapsed.TotalMilliseconds:F2} ms ({parallelResult.DegreeOfParallelism} workers, {parallelResult.Frame.Width}x{parallelResult.Frame.Height}, {BenchmarkMaxIterations} iterations)");
        WriteConsoleLine(0, top + 3, $"Calculation saved:           {savedMilliseconds:F2} ms | Speedup: {speedup:F2}x");
        WriteConsoleLine(0, top + 4, $"Single-threaded rendering:   {singleThreadedRenderElapsed.TotalMilliseconds:F2} ms");
        WriteConsoleLine(0, top + 5, $"Parallel result rendering:   {parallelRenderElapsed.TotalMilliseconds:F2} ms");
    }

    private static void WriteConsoleLine(int left, int top, string text)
    {
        if (top < 0 || top >= System.Console.BufferHeight)
        {
            return;
        }

        System.Console.SetCursorPosition(left, top);
        System.Console.Write(text.Length >= System.Console.WindowWidth ? text[..Math.Max(0, System.Console.WindowWidth - 1)] : text);
    }

    private static void LogComparison(
        MandelbrotCalculationResult singleThreadedResult,
        MandelbrotCalculationResult parallelResult,
        ILogger logger)
    {
        var savedMilliseconds = singleThreadedResult.Elapsed.TotalMilliseconds - parallelResult.Elapsed.TotalMilliseconds;
        var speedup = parallelResult.Elapsed.TotalMilliseconds <= 0
            ? 0
            : singleThreadedResult.Elapsed.TotalMilliseconds / parallelResult.Elapsed.TotalMilliseconds;

        logger.Information(
            "Calculation comparison: SingleThreaded={SingleThreadedMs:F2} ms, Parallel={ParallelMs:F2} ms, Saved={SavedMs:F2} ms, Speedup={Speedup:F2}x, ParallelWorkers={ParallelWorkers}, Points={Points}.",
            singleThreadedResult.Elapsed.TotalMilliseconds,
            parallelResult.Elapsed.TotalMilliseconds,
            savedMilliseconds,
            speedup,
            parallelResult.DegreeOfParallelism,
            singleThreadedResult.Frame.PointCount);
    }

    private static ILogger CreateLogger()
    {
        try
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(
                    LoggingPaths.GetDefaultLogFilePath(),
                    rollingInterval: RollingInterval.Infinite,
                    shared: false)
                .CreateLogger();
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException)
        {
            var fallbackLogger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .CreateLogger();

            fallbackLogger.Warning(ex, "File logging is unavailable. Logging output is disabled for this run.");
            return fallbackLogger;
        }
    }
}
