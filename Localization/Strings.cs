namespace BetterCompactAntigravity.Localization;

/// <summary>
/// User-facing UI strings, Turkish by default. <see cref="LocManager"/> reflection-writes these
/// fields to <c>lang.tr.xml</c> when it is missing, and loads matching XML entries when a language
/// is selected. Keep each public string field mutable so the existing loader can update it.
/// Add the same key and matching format placeholders to lang.en.xml for every new field.
/// </summary>
internal static class Strings
{
    private const bool IsLogEnabled = true;

    // Application.
    public static string AppTitle = "BetterCompact Antigravity";
    public static string AppDescription = "Antigravity sohbetini araç trafiğini sayarak Markdown'a aktarır.";

    // Language.
    public static string SettingLanguage = "Dil";
    public static string LanguageSystem = "Windows'u izle";

    // Command line and processing.
    public static string HelpTitle = "BetterCompact Antigravity - Antigravity sohbetini Markdown'a aktarır.";
    public static string HelpUsage = "Kullanım: BetterCompactAntigravity <sohbet-kimliği|sohbet.db> [-o <çıktı.md|klasör>]";
    public static string HelpInput = "Girdi: Antigravity sohbet kimliği veya conversations klasöründeki SQLite dosyası.";
    public static string HelpOutput = "-o: Markdown dosyası ya da çıktı klasörü.";
    public static string HelpDefaultOutput = "Varsayılan çıktı: program klasöründeki UserData/Conversations.";
    public static string InputRequired = "Bir Antigravity sohbet kimliği veya veritabanı yolu gerekiyor.";
    public static string OutputPathRequired = "-o seçeneğinden sonra çıktı yolu gerekiyor.";
    public static string UnknownOption = "Bilinmeyen seçenek: {0}. --help ile kullanımı göster.";
    public static string OneInputOnly = "Yalnızca bir sohbet girdisi kabul edilir.";
    public static string InputPrompt = "Antigravity sohbet kimliğini veya .db yolunu gir: ";
    public static string OutputPrompt = "Markdown dosyası veya klasör yolu (boş bırakırsan varsayılan konum): ";
    public static string NoInput = "Girdi verilmedi.";
    public static string NoConversation = "Sohbet kaydı bulunamadı: {0}";
    public static string NoMessages = "Sohbet veritabanında aktarılabilir mesaj bulunamadı.";
    public static string DatabaseOpenFailed = "Veritabanı açılamadı: {0}\n{1}";
    public static string SqliteOpenFailed = "Windows SQLite kitaplığı veritabanını açamadı.";
    public static string SqliteUnknownError = "SQLite işlemi başarısız oldu.";
    public static string SummaryDatabaseMissing = "Antigravity sohbet özeti veritabanı bulunamadı: {0}";
    public static string HelpNotAvailable = "Yardım için --help kullan.";
    public static string Reading = "Antigravity sohbeti okunuyor: {0}";
    public static string Written = "Markdown yazıldı: {0}";
    public static string Failed = "İşlem tamamlanamadı: {0}";
    public static string KeptHeading = "  tutulan";
    public static string UserCount = "    kullanıcı iletisi  {0}";
    public static string ReplyCount = "    asistan yanıtı     {0}";
    public static string DroppedHeading = "  çıkarılan";
    public static string ToolCount = "    araç adımı        {0}";
    public static string ActivityCount = "    görev/etkinlik     {0}";
    public static string GenerationCount = "    üretim adımı      {0}";
    public static string SummaryCount = "    bağlam özeti       {0}";
    public static string StepCount = "    toplam adım        {0}";
    public static string SizeSummary = "{0} -> {1} ({2}% of the original)";
    public static string SessionLabel = "Conversation";
    public static string LastActivityLabel = "Son etkinlik";
    public static string UserHeading = "## User";
    public static string AssistantHeading = "## Antigravity";
    public static string ToolActivityFormat = "{0} tool calls";
}
