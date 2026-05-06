namespace Mandelbrot.Application;

internal sealed class MandelbrotCoordinateMapper
{
    private readonly MandelbrotOptions _options;
    private readonly double _halfWidth;
    private readonly double _halfHeight;
    private readonly double _scaleX;
    private readonly double _scaleY;

    public MandelbrotCoordinateMapper(MandelbrotOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
        _halfWidth = options.Width / 2.0;
        _halfHeight = options.Height / 2.0;
        _scaleX = options.Scale / options.Width;
        _scaleY = _scaleX / options.CharacterAspectRatio;
    }

    public (double Real, double Imaginary) Map(int x, int y)
    {
        return (
            _options.CenterX + ((x - _halfWidth) * _scaleX),
            _options.CenterY + ((y - _halfHeight) * _scaleY));
    }
}
