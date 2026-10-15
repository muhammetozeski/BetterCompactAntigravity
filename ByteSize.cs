namespace BetterCompactAntigravity;

/// <summary>
/// Writes byte counts the way a person reads them. Used both by what the program prints and by the
/// placeholder it leaves in place of a message it was told to leave out.
/// </summary>
static class ByteSize
{
    public const int BytesPerKilobyte = 1024;
    public const int BytesPerMegabyte = BytesPerKilobyte * BytesPerKilobyte;

    /// <summary>
    /// Turns a byte count into the largest unit it still reaches.
    /// </summary>
    /// <param name="bytes">The size to describe.</param>
    /// <returns>The size with its unit, for example <c>197 KB</c>.</returns>
    public static string Describe(long bytes) => bytes switch
    {
        >= BytesPerMegabyte => $"{(double)bytes / BytesPerMegabyte:0.##} MB",
        >= BytesPerKilobyte => $"{bytes / BytesPerKilobyte} KB",
        _ => $"{bytes} B"
    };
}
