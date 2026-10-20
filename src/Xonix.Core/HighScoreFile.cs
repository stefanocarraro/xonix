namespace Xonix.Core;

/// <summary>
/// Keeps the best score in a plain text file for desktop hosts.
/// </summary>
/// <param name="path">File holding the score.</param>
public sealed class HighScoreFile(string path)
{
    /// <summary>
    /// Gets the per-user default location: <c>%AppData%\xonix</c> on Windows, <c>~/.config/xonix</c> on Linux.
    /// </summary>
    public static string DefaultPath => System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "xonix", "highscore.txt");

    /// <summary>Gets the file holding the score.</summary>
    public string Path { get; } = path;

    /// <summary>
    /// Reads the saved score.
    /// </summary>
    /// <remarks>A missing file means no score yet; an unreadable or corrupt one is reported on stderr and ignored.</remarks>
    /// <returns>The saved score, or 0.</returns>
    public int Load()
    {
        if (!File.Exists(Path))
        {
            return 0;
        }

        try
        {
            string text = File.ReadAllText(Path).Trim();
            if (int.TryParse(text, out int score) && score >= 0)
            {
                return score;
            }

            Console.Error.WriteLine($"High score file {Path} is corrupt ('{text}'); starting from 0");
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"Cannot read high score file {Path}: {error.Message}");
        }

        return 0;
    }

    /// <summary>
    /// Writes the score, creating the folder when needed; failures are reported on stderr so play continues.
    /// </summary>
    /// <param name="score">Score to store.</param>
    public void Save(int score)
    {
        try
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
            File.WriteAllText(Path, $"{score}");
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"Cannot save high score to {Path}: {error.Message}");
        }
    }
}
