using System.Globalization;

namespace BetterCompactAntigravity;

/// <summary>
/// Prints localized conversion progress and counts from one Antigravity conversation.
/// </summary>
static class ConsoleReport
{
    const int PercentageScale = 100;

    /// <summary>Prints the conversion input.</summary>
    public static void Reading(string path) => Write("[*] ", ConsoleColor.DarkCyan, string.Format(Strings.Reading, path));

    /// <summary>Prints a successful output path.</summary>
    public static void Written(string path) => Write("[+] ", ConsoleColor.Green, string.Format(Strings.Written, path));

    /// <summary>Prints a failure message.</summary>
    public static void Error(string message) => Write("[!] ", ConsoleColor.Red, message);

    /// <summary>Prints kept and dropped counts and the input-to-output size change.</summary>
    public static void Summarise(TranscriptStatistics statistics, long inputBytes, long outputBytes)
    {
        Console.WriteLine();
        Console.WriteLine(Strings.KeptHeading);
        Console.WriteLine(string.Format(Strings.UserCount, statistics.UserMessages));
        Console.WriteLine(string.Format(Strings.ReplyCount, statistics.Replies));
        Console.WriteLine(Strings.DroppedHeading);
        Console.WriteLine(string.Format(Strings.ToolCount, statistics.ToolSteps));
        Console.WriteLine(string.Format(Strings.ActivityCount, statistics.ActivitySteps));
        Console.WriteLine(string.Format(Strings.GenerationCount, statistics.GenerationSteps));
        Console.WriteLine(string.Format(Strings.SummaryCount, statistics.SummarySteps));
        Console.WriteLine(string.Format(Strings.StepCount, statistics.TotalSteps));
        double ratio = inputBytes == 0 ? 0 : (double)outputBytes / inputBytes * PercentageScale;
        Console.WriteLine();
        Console.WriteLine(string.Format(Strings.SizeSummary, ByteSize.Describe(inputBytes), ByteSize.Describe(outputBytes), ratio.ToString("0.#", CultureInfo.InvariantCulture)));
    }

    static void Write(string prefix, ConsoleColor colour, string message)
    {
        Console.ForegroundColor = colour;
        Console.Write(prefix);
        Console.ResetColor();
        Console.WriteLine(message);
    }
}
