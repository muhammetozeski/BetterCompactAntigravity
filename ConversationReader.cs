using System.Globalization;
using System.Text;
using System.Text.Json;

namespace BetterCompactAntigravity;

/// <summary>
/// Reads Antigravity's indexed conversation text and step counts without changing either database.
/// </summary>
sealed class ConversationReader
{
    const int ToolCallStepType = 132;
    const int ActivityStepType = 101;
    const int GenerationStepType = 15;
    const int SummaryStepType = 23;

    static readonly UTF8Encoding StrictUtf8 = new(false, true);

    /// <summary>
    /// Reads all user messages and assistant replies for the requested Antigravity conversation.
    /// </summary>
    /// <param name="conversationId">The Antigravity conversation GUID.</param>
    /// <returns>The retained text, ordered by source step, and counts for excluded step types.</returns>
    public TranscriptDocument Read(string conversationId, string? conversationPath = null)
    {
        string antigravityFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gemini", "antigravity");
        conversationPath ??= Path.Combine(antigravityFolder, "conversations", conversationId + ".db");
        string summariesPath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(conversationPath)!)!, "conversation_summaries.db");

        if (!File.Exists(conversationPath)) throw new FileNotFoundException(string.Format(Strings.NoConversation, conversationPath));
        if (!File.Exists(summariesPath)) throw new FileNotFoundException(string.Format(Strings.SummaryDatabaseMissing, summariesPath));

        (string title, DateTimeOffset? lastActivity, int totalSteps) = ReadSummary(summariesPath, conversationId);
        List<TranscriptMessage> messages = ReadMessages(summariesPath, conversationId);
        Dictionary<int, int> stepCounts = ReadStepCounts(conversationPath);
        int userCount = messages.Count(message => message.Kind == MessageKind.User);
        int replyCount = messages.Count(message => message.Kind == MessageKind.Assistant);
        if (userCount == 0 && replyCount == 0) throw new InvalidDataException(Strings.NoMessages);

        int[] toolSteps = ReadToolStepIndexes(conversationPath);
        List<TranscriptMessage> withToolActivity = InsertToolActivity(messages, toolSteps);
        TranscriptStatistics statistics = new(
            userCount,
            replyCount,
            stepCounts.GetValueOrDefault(ToolCallStepType),
            stepCounts.GetValueOrDefault(ActivityStepType),
            stepCounts.GetValueOrDefault(GenerationStepType),
            stepCounts.GetValueOrDefault(SummaryStepType),
            totalSteps);

        return new TranscriptDocument(title, conversationId, lastActivity, withToolActivity, statistics);
    }

    static (string Title, DateTimeOffset? LastActivity, int TotalSteps) ReadSummary(string path, string conversationId)
    {
        using NativeSqlite database = NativeSqlite.OpenReadOnly(path);
        using NativeSqlite.SqliteStatement statement = database.Prepare(
            "SELECT title, last_user_input_time, step_count FROM conversation_summaries WHERE conversation_id = ?;",
            conversationId);
        if (!statement.Read()) throw new FileNotFoundException(string.Format(Strings.NoConversation, conversationId));

        string title = statement.GetText(0);
        string timestamp = statement.GetText(1);
        DateTimeOffset? lastActivity = DateTimeOffset.TryParse(
            timestamp,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal,
            out DateTimeOffset parsed) ? parsed : null;
        return (title.Length == 0 ? conversationId : title, lastActivity, checked((int)statement.GetInt64(2)));
    }

    static List<TranscriptMessage> ReadMessages(string path, string conversationId)
    {
        using NativeSqlite database = NativeSqlite.OpenReadOnly(path);
        using NativeSqlite.SqliteStatement statement = database.Prepare(
            "SELECT c1, c8, c9, c10, c13, c14 FROM conversations_fts_content WHERE c0 = ? AND c1 >= 0 ORDER BY c1;",
            conversationId);
        List<TranscriptMessage> messages = [];

        while (statement.Read())
        {
            string userBody = statement.GetText(4);
            string assistantBody = statement.GetText(5);
            messages.AddRange(ReadBody(userBody, statement.GetText(1), MessageKind.User));
            messages.AddRange(ReadBody(assistantBody, statement.GetText(2), MessageKind.Assistant));
        }

        return messages.OrderBy(message => message.StepIndex).ToList();
    }

    static IEnumerable<TranscriptMessage> ReadBody(string body, string offsetsJson, MessageKind kind)
    {
        if (body.Length == 0 || offsetsJson.Length == 0) yield break;
        byte[] bodyBytes = StrictUtf8.GetBytes(body);
        List<StepOffset> offsets = ParseOffsets(offsetsJson)
            .Where(offset => offset.ByteOffset >= 0 && offset.ByteOffset < bodyBytes.Length)
            .OrderBy(offset => offset.ByteOffset)
            .ThenBy(offset => offset.StepIndex)
            .ToList();

        for (int index = 0; index < offsets.Count; index++)
        {
            StepOffset offset = offsets[index];
            int end = bodyBytes.Length;
            for (int next = index + 1; next < offsets.Count; next++)
            {
                if (offsets[next].ByteOffset > offset.ByteOffset)
                {
                    end = offsets[next].ByteOffset;
                    break;
                }
            }
            if (end <= offset.ByteOffset) continue;

            string text;
            try { text = StrictUtf8.GetString(bodyBytes, offset.ByteOffset, end - offset.ByteOffset); }
            catch (DecoderFallbackException exception)
            {
                throw new InvalidDataException($"Conversation text offset {offset.ByteOffset} does not align to a UTF-8 character boundary.", exception);
            }
            if (text.Length > 0) yield return new TranscriptMessage(offset.StepIndex, kind, text);
        }
    }

    static List<StepOffset> ParseOffsets(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        List<StepOffset> offsets = [];
        foreach (JsonElement pair in document.RootElement.EnumerateArray())
        {
            if (pair.ValueKind != JsonValueKind.Array || pair.GetArrayLength() != 2) continue;
            offsets.Add(new StepOffset(pair[0].GetInt32(), pair[1].GetInt32()));
        }
        return offsets;
    }

    static Dictionary<int, int> ReadStepCounts(string path)
    {
        using NativeSqlite database = NativeSqlite.OpenReadOnly(path);
        using NativeSqlite.SqliteStatement statement = database.Prepare("SELECT step_type, COUNT(*) FROM steps GROUP BY step_type;");
        Dictionary<int, int> counts = [];
        while (statement.Read()) counts[checked((int)statement.GetInt64(0))] = checked((int)statement.GetInt64(1));
        return counts;
    }

    static int[] ReadToolStepIndexes(string path)
    {
        using NativeSqlite database = NativeSqlite.OpenReadOnly(path);
        using NativeSqlite.SqliteStatement statement = database.Prepare("SELECT idx FROM steps WHERE step_type = ? ORDER BY idx;", ToolCallStepType.ToString(CultureInfo.InvariantCulture));
        List<int> indexes = [];
        while (statement.Read()) indexes.Add(checked((int)statement.GetInt64(0)));
        return [.. indexes];
    }

    static List<TranscriptMessage> InsertToolActivity(List<TranscriptMessage> messages, int[] toolSteps)
    {
        List<TranscriptMessage> result = [];
        int toolIndex = 0;
        int pendingTools = 0;

        foreach (TranscriptMessage message in messages)
        {
            while (toolIndex < toolSteps.Length && toolSteps[toolIndex] < message.StepIndex)
            {
                pendingTools++;
                toolIndex++;
            }
            if (pendingTools > 0)
            {
                result.Add(new TranscriptMessage(message.StepIndex, MessageKind.ToolActivity, string.Format(Strings.ToolActivityFormat, pendingTools)));
                pendingTools = 0;
            }
            result.Add(message);
        }

        while (toolIndex < toolSteps.Length)
        {
            pendingTools++;
            toolIndex++;
        }
        if (pendingTools > 0) result.Add(new TranscriptMessage(int.MaxValue, MessageKind.ToolActivity, string.Format(Strings.ToolActivityFormat, pendingTools)));
        return result;
    }

    readonly record struct StepOffset(int StepIndex, int ByteOffset);
}
