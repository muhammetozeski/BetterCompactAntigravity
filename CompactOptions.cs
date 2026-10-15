namespace BetterCompactAntigravity;

/// <summary>
/// Holds the Antigravity conversation identifier or database path and the requested Markdown destination.
/// </summary>
/// <param name="Input">A conversation identifier or path to its SQLite database.</param>
/// <param name="Output">A Markdown file path or a folder where the generated file will be placed.</param>
sealed record CompactOptions(string Input, string Output);

/// <summary>
/// Parses command-line arguments for one Antigravity conversation export.
/// </summary>
static class CommandLine
{
    /// <summary>
    /// Reads one input and an optional output path from the process arguments.
    /// </summary>
    /// <param name="arguments">Arguments passed to the application.</param>
    /// <param name="options">The parsed options, or null when help was requested or parsing failed.</param>
    /// <param name="error">A localized parse error, or an empty string when none occurred.</param>
    /// <param name="showHelp">True when the caller should print help and exit successfully.</param>
    /// <returns>True when conversion should proceed.</returns>
    public static bool TryParse(string[] arguments, out CompactOptions? options, out string error, out bool showHelp)
    {
        options = null;
        error = string.Empty;
        showHelp = false;
        string input = string.Empty;
        string output = string.Empty;

        for (int index = 0; index < arguments.Length; index++)
        {
            switch (arguments[index])
            {
                case "-h" or "--help" or "/?":
                    showHelp = true;
                    return false;
                case "-o" or "--output":
                    if (index + 1 >= arguments.Length)
                    {
                        error = Strings.OutputPathRequired;
                        return false;
                    }
                    output = arguments[++index].Trim('"');
                    break;
                default:
                    if (arguments[index].StartsWith('-'))
                    {
                        error = string.Format(Strings.UnknownOption, arguments[index]);
                        return false;
                    }
                    if (input.Length > 0)
                    {
                        error = Strings.OneInputOnly;
                        return false;
                    }
                    input = arguments[index].Trim('"');
                    break;
            }
        }

        if (input.Length == 0)
        {
            error = Strings.InputRequired;
            return false;
        }

        options = new CompactOptions(input, output);
        return true;
    }

    /// <summary>
    /// Writes the supported command syntax and Antigravity database location.
    /// </summary>
    public static void PrintHelp()
    {
        Console.WriteLine(Strings.HelpTitle);
        Console.WriteLine();
        Console.WriteLine(Strings.HelpUsage);
        Console.WriteLine(Strings.HelpInput);
        Console.WriteLine(Strings.HelpOutput);
        Console.WriteLine(Strings.HelpDefaultOutput);
    }
}
