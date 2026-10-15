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

        int index = 0;
        while (index < document.Messages.Count)
        {
            TranscriptMessage message = document.Messages[index];
            if (message.Kind == MessageKind.User)
            {
                AppendMessage(builder, Strings.UserHeading, message.Text);
                index++;
                continue;
            }

            List<string> replies = [];
            int toolCalls = 0;
            while (index < document.Messages.Count && document.Messages[index].Kind != MessageKind.User)
            {
                TranscriptMessage part = document.Messages[index++];
                if (part.Kind == MessageKind.ToolActivity)
                    toolCalls += part.ActivityCount;
                else if (!string.IsNullOrWhiteSpace(part.Text))
                    replies.Add(part.Text);
            }

            if (toolCalls > 0)
            {
                builder.AppendLine($"> {string.Format(Strings.ToolActivityFormat, toolCalls)}");
                builder.AppendLine();
            }

            if (replies.Count > 0)
                AppendMessage(builder, Strings.AssistantHeading, string.Join(Environment.NewLine + Environment.NewLine, replies));
        }
        return builder.ToString();
    }

    static void AppendMessage(StringBuilder builder, string heading, string text)
    {
        builder.AppendLine(heading);
        builder.AppendLine();
        builder.AppendLine(text);
        builder.AppendLine();
    }
}
