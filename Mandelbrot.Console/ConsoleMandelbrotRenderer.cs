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

    public void Render(MandelbrotFrame frame, int maxIterations)
    {
        ArgumentNullException.ThrowIfNull(frame);

        if (maxIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIterations), maxIterations, "Maximum iterations must be greater than zero.");
        }

        var output = new StringBuilder(frame.Height * (frame.Width + Environment.NewLine.Length));

        for (var y = 0; y < frame.Height; y++)
        {
            for (var x = 0; x < frame.Width; x++)
            {
                var point = frame.GetPoint(x, y);
                output.Append(_palette.GetCharacter(point.Iterations, maxIterations, point.IsInSet));
            }

            output.AppendLine();
        }

        System.Console.SetCursorPosition(0, 0);
        System.Console.Write(output.ToString());
    }
}
