using System.Text;
using Mandelbrot.Application;

namespace Mandelbrot.Console;

internal sealed class ConsoleMandelbrotRenderer
{
    private readonly CharacterPalette _palette;

    public ConsoleMandelbrotRenderer(CharacterPalette palette)
    {
        _palette = palette ?? throw new ArgumentNullException(nameof(palette));
    }

    public void Render(MandelbrotFrame frame, int maxIterations, int left = 0, int top = 0, string? title = null)
    {
        ArgumentNullException.ThrowIfNull(frame);

        if (maxIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIterations), maxIterations, "Maximum iterations must be greater than zero.");
        }

        var currentTop = top;

        if (!string.IsNullOrWhiteSpace(title))
        {
            WriteAt(left, currentTop++, title.Length > frame.Width ? title[..frame.Width] : title.PadRight(frame.Width));
        }

        for (var y = 0; y < frame.Height; y++)
        {
            var output = new StringBuilder(frame.Width);

            for (var x = 0; x < frame.Width; x++)
            {
                var point = frame.GetPoint(x, y);
                output.Append(_palette.GetCharacter(point.Iterations, maxIterations, point.IsInSet));
            }

            WriteAt(left, currentTop++, output.ToString());
        }
    }

    private static void WriteAt(int left, int top, string text)
    {
        if (left < 0 || top < 0 || left >= System.Console.BufferWidth || top >= System.Console.BufferHeight)
        {
            return;
        }

        var availableWidth = System.Console.BufferWidth - left;
        System.Console.SetCursorPosition(left, top);
        System.Console.Write(text.Length > availableWidth ? text[..availableWidth] : text);
    }
}
