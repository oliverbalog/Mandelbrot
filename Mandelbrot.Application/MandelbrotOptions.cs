namespace Mandelbrot.Application;

public sealed class MandelbrotOptions
{
    public const int DefaultMaxIterations = 400;
    public const double DefaultCenterX = -0.5;
    public const double DefaultCenterY = 0.0;
    public const double DefaultScale = 4.0;
    public const double DefaultCharacterAspectRatio = 0.5;

    public MandelbrotOptions(
        int width,
        int height,
        int maxIterations = DefaultMaxIterations,
        double centerX = DefaultCenterX,
        double centerY = DefaultCenterY,
        double scale = DefaultScale,
        double characterAspectRatio = DefaultCharacterAspectRatio)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be greater than zero.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be greater than zero.");
        }

        if (maxIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIterations), maxIterations, "Maximum iterations must be greater than zero.");
        }

        if (!double.IsFinite(centerX))
        {
            throw new ArgumentOutOfRangeException(nameof(centerX), centerX, "Center X must be finite.");
        }

        if (!double.IsFinite(centerY))
        {
            throw new ArgumentOutOfRangeException(nameof(centerY), centerY, "Center Y must be finite.");
        }

        if (!double.IsFinite(scale) || scale <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(scale), scale, "Scale must be finite and greater than zero.");
        }

        if (!double.IsFinite(characterAspectRatio) || characterAspectRatio <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(characterAspectRatio), characterAspectRatio, "Character aspect ratio must be finite and greater than zero.");
        }

        Width = width;
        Height = height;
        MaxIterations = maxIterations;
        CenterX = centerX;
        CenterY = centerY;
        Scale = scale;
        CharacterAspectRatio = characterAspectRatio;
    }

    public int Width { get; }

    public int Height { get; }

    public int MaxIterations { get; }

    public double CenterX { get; }

    public double CenterY { get; }

    public double Scale { get; }

    public double CharacterAspectRatio { get; }
}
