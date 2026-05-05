namespace Mandelbrot.Console;

internal sealed class CharacterPalette
{
    private readonly string _characters;

    public CharacterPalette(string characters)
    {
        if (string.IsNullOrWhiteSpace(characters))
        {
            throw new ArgumentException("Palette must contain at least one visible character.", nameof(characters));
        }

        _characters = characters;
    }

    public char GetCharacter(int iterations, int maxIterations, bool isInSet)
    {
        if (maxIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIterations), maxIterations, "Maximum iterations must be greater than zero.");
        }

        if (isInSet)
        {
            return _characters[^1];
        }

        var normalized = Math.Clamp((double)iterations / maxIterations, 0.0, 1.0);
        var index = (int)Math.Round(normalized * (_characters.Length - 1), MidpointRounding.AwayFromZero);
        return _characters[index];
    }
}
