using System.Globalization;
using System.Text;

namespace BetterCompactAntigravity;

/// <summary>
/// Writes a BetterCompact-style Markdown transcript from retained Antigravity messages and step counts.
/// </summary>
static class MarkdownComposer
{
    /// <summary>
    /// Builds the document header, kept messages and tool activity lines without rewriting message text.
    /// </summary>
    /// <param name="document">The extracted Antigravity conversation.</param>
    /// <returns>The Markdown content ready to save as UTF-8.</returns>
    public static string Compose(TranscriptDocument document)
    {
        StringBuilder builder = new();
        builder.AppendLine($"# {document.Title.ReplaceLineEndings(" ")}");
        builder.AppendLine();
        builder.AppendLine($"> {Strings.SessionLabel} `{document.ConversationId}`  ");
        if (document.LastActivity != null)
            builder.AppendLine($"> {Strings.LastActivityLabel}: {document.LastActivity.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)}  ");
        TranscriptStatistics statistics = document.Statistics;
        builder.AppendLine($"> {statistics.UserMessages} user messages, {statistics.Replies} replies, kept from {statistics.TotalSteps} steps  ");
        builder.AppendLine($"> Dropped: {statistics.ToolSteps} tool steps, {statistics.ActivitySteps} task/activity, {statistics.GenerationSteps} generation steps, {statistics.SummarySteps} context summaries");
        builder.AppendLine();
        builder.AppendLine("---");
        builder.AppendLine();

        foreach (TranscriptMessage message in document.Messages)
        {
            if (message.Kind == MessageKind.ToolActivity)
            {
                builder.AppendLine($"> {message.Text}");
                builder.AppendLine();
                continue;
            }

            builder.AppendLine(message.Kind == MessageKind.User ? Strings.UserHeading : Strings.AssistantHeading);
            builder.AppendLine();
            builder.AppendLine(message.Text);
            builder.AppendLine();
        }
        return builder.ToString();
    }
}
