namespace Mandelbrot.Application;

public readonly record struct MandelbrotPoint(int X, int Y, int Iterations, bool IsInSet);
