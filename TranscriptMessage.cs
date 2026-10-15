namespace BetterCompactAntigravity;

/// <summary>
/// One user message or assistant reply, retaining its original UTF-8 text and Antigravity step order.
/// </summary>
/// <param name="StepIndex">The source step that began the message.</param>
/// <param name="Kind">Whether the text came from the user or Antigravity.</param>
/// <param name="Text">The message body from Antigravity's searchable conversation record.</param>
sealed record TranscriptMessage(int StepIndex, MessageKind Kind, string Text);

/// <summary>
/// Identifies the speaker for one retained conversation message.
/// </summary>
enum MessageKind
{
    /// <summary>Text entered by the user.</summary>
    User,

    /// <summary>Text shown as an Antigravity reply.</summary>
    Assistant,

    /// <summary>A count of tool steps between two retained messages.</summary>
    ToolActivity
}

/// <summary>
/// Counts retained conversation messages and Antigravity activity excluded from the transcript.
/// </summary>
/// <param name="UserMessages">User messages retained from the full-text index.</param>
/// <param name="Replies">Assistant replies retained from the full-text index.</param>
/// <param name="ToolSteps">Tool invocation steps excluded from the Markdown body.</param>
/// <param name="ActivitySteps">Task notifications and activity steps excluded from the body.</param>
/// <param name="GenerationSteps">Internal generation steps excluded from the body.</param>
/// <param name="SummarySteps">Context summary steps excluded from the body.</param>
/// <param name="TotalSteps">All rows stored for this conversation.</param>
sealed record TranscriptStatistics(
    int UserMessages,
    int Replies,
    int ToolSteps,
    int ActivitySteps,
    int GenerationSteps,
    int SummarySteps,
    int TotalSteps);

/// <summary>
/// A conversation after extraction: its Antigravity title, identifier, retained messages and dropped-step counts.
/// </summary>
/// <param name="Title">The title from Antigravity's conversation summary.</param>
/// <param name="ConversationId">The identifier used by the conversation database.</param>
/// <param name="LastActivity">The last recorded user-input time, when available.</param>
/// <param name="Messages">User and assistant text in original step order, with tool activity markers inserted.</param>
/// <param name="Statistics">Counts that explain how much database activity the Markdown omits.</param>
sealed record TranscriptDocument(
    string Title,
    string ConversationId,
    DateTimeOffset? LastActivity,
    IReadOnlyList<TranscriptMessage> Messages,
    TranscriptStatistics Statistics);
