namespace Mandelbrot.Application;

internal static class MandelbrotIterationEngine
{
    public static int CalculateEscapeIterations(double real, double imaginary, int maxIterations)
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
