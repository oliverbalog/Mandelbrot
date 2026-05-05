namespace Mandelbrot.Application;

public sealed class MandelbrotFrame
{
    private readonly MandelbrotPoint[] _points;

    public MandelbrotFrame(int width, int height, MandelbrotPoint[] points)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be greater than zero.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be greater than zero.");
        }

        ArgumentNullException.ThrowIfNull(points);

        if (points.Length != width * height)
        {
            throw new ArgumentException("Point count must match the frame dimensions.", nameof(points));
        }

        Width = width;
        Height = height;
        _points = points;
    }

    public int Width { get; }

    public int Height { get; }

    public MandelbrotPoint GetPoint(int x, int y)
    {
        if ((uint)x >= (uint)Width)
        {
            throw new ArgumentOutOfRangeException(nameof(x), x, "X must be inside the frame.");
        }

        if ((uint)y >= (uint)Height)
        {
            throw new ArgumentOutOfRangeException(nameof(y), y, "Y must be inside the frame.");
        }

        return _points[(y * Width) + x];
    }
}
