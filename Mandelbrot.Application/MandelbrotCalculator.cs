using Serilog;

namespace Mandelbrot.Application;

public static class MandelbrotCalculator
{
    private const int DefaultBlockSize = 32;

    public static MandelbrotFrame Calculate(MandelbrotOptions options, ILogger? logger = null)
    {
        return CalculateSingleThreaded(options, logger).Frame;
    }

    public static MandelbrotCalculationResult CalculateSingleThreaded(MandelbrotOptions options, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        logger ??= new LoggerConfiguration().CreateLogger();
        logger.Information(
            "Starting single-threaded Mandelbrot calculation. Width={Width}, Height={Height}, MaxIterations={MaxIterations}, CenterX={CenterX}, CenterY={CenterY}, Scale={Scale}.",
            options.Width,
            options.Height,
            options.MaxIterations,
            options.CenterX,
            options.CenterY,
            options.Scale);

        using var timedOperation = TimedOperation.Start("Single-threaded Mandelbrot calculation", logger);

        var points = new MandelbrotPoint[options.Width * options.Height];
        var coordinateMapper = new MandelbrotCoordinateMapper(options);
        var pointIndex = 0;

        for (var y = 0; y < options.Height; y++)
        {
            for (var x = 0; x < options.Width; x++)
            {
                points[pointIndex++] = CalculatePoint(options, coordinateMapper, x, y);
            }
        }

        var frame = new MandelbrotFrame(options.Width, options.Height, points);

        return new MandelbrotCalculationResult(
            frame,
            timedOperation.Elapsed,
            "Single-threaded",
            1);
    }

    public static MandelbrotCalculationResult CalculateParallel(
        MandelbrotOptions options,
        ILogger? logger = null,
        int? degreeOfParallelism = null,
        int blockSize = DefaultBlockSize)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (blockSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(blockSize), blockSize, "Block size must be greater than zero.");
        }

        logger ??= new LoggerConfiguration().CreateLogger();

        var workerCount = degreeOfParallelism ?? Environment.ProcessorCount;
        if (workerCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(degreeOfParallelism), degreeOfParallelism, "Degree of parallelism must be greater than zero.");
        }

        var blocks = MandelbrotBlockScheduler.CreateBlocks(options.Width, options.Height, blockSize);

        logger.Information(
            "Starting parallel Mandelbrot calculation. Width={Width}, Height={Height}, MaxIterations={MaxIterations}, CenterX={CenterX}, CenterY={CenterY}, Scale={Scale}, Workers={Workers}, BlockSize={BlockSize}, Blocks={Blocks}.",
            options.Width,
            options.Height,
            options.MaxIterations,
            options.CenterX,
            options.CenterY,
            options.Scale,
            workerCount,
            blockSize,
            blocks.Count);

        using var timedOperation = TimedOperation.Start("Parallel Mandelbrot calculation", logger);

        var points = new MandelbrotPoint[options.Width * options.Height];
        var coordinateMapper = new MandelbrotCoordinateMapper(options);
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = workerCount
        };

        Parallel.ForEach(
            blocks,
            parallelOptions,
            block => CalculateBlock(options, coordinateMapper, points, block));

        var frame = new MandelbrotFrame(options.Width, options.Height, points);

        return new MandelbrotCalculationResult(
            frame,
            timedOperation.Elapsed,
            "Parallel block work pool",
            workerCount);
    }

    private static void CalculateBlock(
        MandelbrotOptions options,
        MandelbrotCoordinateMapper coordinateMapper,
        MandelbrotPoint[] points,
        MandelbrotBlock block)
    {
        for (var y = block.Y; y < block.Y + block.Height; y++)
        {
            for (var x = block.X; x < block.X + block.Width; x++)
            {
                points[(y * options.Width) + x] = CalculatePoint(options, coordinateMapper, x, y);
            }
        }
    }

    private static MandelbrotPoint CalculatePoint(MandelbrotOptions options, MandelbrotCoordinateMapper coordinateMapper, int x, int y)
    {
        var (real, imaginary) = coordinateMapper.Map(x, y);
        var iterations = MandelbrotIterationEngine.CalculateEscapeIterations(real, imaginary, options.MaxIterations);

        return new MandelbrotPoint(
            x,
            y,
            iterations,
            iterations >= options.MaxIterations);
    }
}
