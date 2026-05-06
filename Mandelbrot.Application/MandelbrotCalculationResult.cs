namespace Mandelbrot.Application;

public sealed record MandelbrotCalculationResult(
    MandelbrotFrame Frame,
    TimeSpan Elapsed,
    string Strategy,
    int DegreeOfParallelism);
