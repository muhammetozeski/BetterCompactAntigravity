using System.Text;

namespace BetterCompactAntigravity;

/// <summary>
/// Resolves one Antigravity conversation, writes its retained messages and prints the dropped-step counts.
/// </summary>
static class Program
{
    const int ExitSuccess = 0;
    const int ExitFailure = 1;
    const string DatabaseExtension = ".db";
    const string OutputSuffix = ".compact.md";

    /// <summary>Runs one conversation export or asks for the conversation identifier interactively.</summary>
    /// <param name="arguments">The conversation identifier or database path and optional output path.</param>
    /// <returns>Zero after a successful export; otherwise a nonzero process exit code.</returns>
    static int Main(string[] arguments)
    {
        Console.OutputEncoding = Encoding.UTF8;
        var initialization = ApplicationServices.Initialize();
        if (!initialization.Succeeded)
        {
            ConsoleReport.Error(initialization.Error?.Message ?? Strings.Failed);
            return ExitFailure;
        }

        bool interactive = arguments.Length == 0;
        if (interactive)
        {
            Console.Write(Strings.InputPrompt);
            string input = Console.ReadLine()?.Trim().Trim('"') ?? string.Empty;
            if (input.Length == 0)
            {
                ConsoleReport.Error(Strings.NoInput);
                return ExitFailure;
            }
            Console.Write(Strings.OutputPrompt);
            string output = Console.ReadLine()?.Trim().Trim('"') ?? string.Empty;
            arguments = output.Length == 0 ? [input] : [input, "--output", output];
        }

        if (!CommandLine.TryParse(arguments, out CompactOptions? options, out string error, out bool showHelp))
        {
            if (showHelp)
            {
                CommandLine.PrintHelp();
                return ExitSuccess;
            }
            ConsoleReport.Error(error);
            if (interactive) CommandLine.PrintHelp();
            return ExitFailure;
        }

        OperationResult<bool> result = RunSafely(() => Convert(options!), operationName: "ConvertAntigravityConversation");
        if (!result.Succeeded)
        {
            ConsoleReport.Error(string.Format(Strings.Failed, result.Error?.Message ?? Strings.SqliteUnknownError));
            return ExitFailure;
        }
        return ExitSuccess;
    }

    static void Convert(CompactOptions options)
    {
        string input = Environment.ExpandEnvironmentVariables(options.Input).Trim('"');
        string conversationId = ResolveConversationId(input);
        string conversationsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gemini", "antigravity", "conversations");
        string databasePath = File.Exists(input) && string.Equals(Path.GetExtension(input), DatabaseExtension, StringComparison.OrdinalIgnoreCase)
            ? Path.GetFullPath(input)
            : Path.Combine(conversationsFolder, conversationId + DatabaseExtension);
        ConsoleReport.Reading(databasePath);

        TranscriptDocument document = new ConversationReader().Read(conversationId, databasePath);
        string markdown = MarkdownComposer.Compose(document).ReplaceLineEndings("\r\n");
        string outputPath = ResolveOutputPath(conversationId, options.Output);
        string? outputFolder = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputFolder)) Directory.CreateDirectory(outputFolder);
        File.WriteAllText(outputPath, markdown, new UTF8Encoding(false));

        ConsoleReport.Summarise(document.Statistics, new FileInfo(databasePath).Length, new FileInfo(outputPath).Length);
        ConsoleReport.Written(outputPath);
    }

    static string ResolveConversationId(string input)
    {
        string value = File.Exists(input) && string.Equals(Path.GetExtension(input), DatabaseExtension, StringComparison.OrdinalIgnoreCase)
            ? Path.GetFileNameWithoutExtension(input)
            : input;
        if (!Guid.TryParse(value, out Guid conversationId))
            throw new ArgumentException(string.Format(Strings.NoConversation, value));
        return conversationId.ToString("D");
    }

    static string ResolveOutputPath(string conversationId, string requestedOutput)
    {
        string defaultFolder = Path.Combine(AppPaths.UserData, "Conversations");
        if (requestedOutput.Length == 0) return Path.Combine(defaultFolder, conversationId + OutputSuffix);

        string output = Path.GetFullPath(Environment.ExpandEnvironmentVariables(requestedOutput));
        return Directory.Exists(output) || string.IsNullOrEmpty(Path.GetExtension(output))
            ? Path.Combine(output, conversationId + OutputSuffix)
            : output;
    }
}
