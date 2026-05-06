namespace Mandelbrot.Application;

internal static class MandelbrotBlockScheduler
{
    public static IReadOnlyList<MandelbrotBlock> CreateBlocks(int width, int height, int blockSize)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be greater than zero.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be greater than zero.");
        }

        if (blockSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(blockSize), blockSize, "Block size must be greater than zero.");
        }

        var blocks = new List<MandelbrotBlock>();

        for (var y = 0; y < height; y += blockSize)
        {
            for (var x = 0; x < width; x += blockSize)
            {
                blocks.Add(new MandelbrotBlock(
                    x,
                    y,
                    Math.Min(blockSize, width - x),
                    Math.Min(blockSize, height - y)));
            }
        }

        return blocks;
    }
}
