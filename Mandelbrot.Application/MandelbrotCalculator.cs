namespace Mandelbrot.Application;

public static class MandelbrotCalculator
{
    public static MandelbrotFrame Calculate(MandelbrotOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var points = new MandelbrotPoint[options.Width * options.Height];
        var halfWidth = options.Width / 2.0;
        var halfHeight = options.Height / 2.0;
        var scaleX = options.Scale / options.Width;
        var scaleY = scaleX / options.CharacterAspectRatio;
        var pointIndex = 0;

        for (var y = 0; y < options.Height; y++)
        {
            for (var x = 0; x < options.Width; x++)
            {
                var real = options.CenterX + ((x - halfWidth) * scaleX);
                var imaginary = options.CenterY + ((y - halfHeight) * scaleY);
                var iterations = CalculateEscapeIterations(real, imaginary, options.MaxIterations);

                points[pointIndex++] = new MandelbrotPoint(
                    x,
                    y,
                    iterations,
                    iterations >= options.MaxIterations);
            }
        }

        return new MandelbrotFrame(options.Width, options.Height, points);
    }

    private static int CalculateEscapeIterations(double real, double imaginary, int maxIterations)
    {
        var zReal = 0.0;
        var zImaginary = 0.0;
        var iterations = 0;

        while (((zReal * zReal) + (zImaginary * zImaginary) <= 4.0) && iterations < maxIterations)
        {
            var nextReal = (zReal * zReal) - (zImaginary * zImaginary) + real;
            zImaginary = (2.0 * zReal * zImaginary) + imaginary;
            zReal = nextReal;
            iterations++;
        }

        return iterations;
    }
}
