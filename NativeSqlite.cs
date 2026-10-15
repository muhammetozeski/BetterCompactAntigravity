using System.Runtime.InteropServices;

namespace BetterCompactAntigravity;

/// <summary>
/// Opens Antigravity's SQLite databases read-only through the SQLite library included with Windows.
/// </summary>
sealed class NativeSqlite : IDisposable
{
    const int OpenReadOnlyFlag = 0x00000001;
    const int ResultRow = 100;
    const int ResultDone = 101;
    const int SqliteTransient = -1;

    readonly IntPtr database;
    bool disposed;

    NativeSqlite(IntPtr database) => this.database = database;

    /// <summary>
    /// Opens a database without changing its contents.
    /// </summary>
    /// <param name="path">The full path to an Antigravity database.</param>
    /// <returns>An open read-only database handle.</returns>
    public static NativeSqlite OpenReadOnly(string path)
    {
        int result = SqliteOpen(path, out IntPtr database, OpenReadOnlyFlag, IntPtr.Zero);
        if (result != 0)
        {
            string error = database == IntPtr.Zero ? Strings.SqliteOpenFailed : ErrorText(database);
            if (database != IntPtr.Zero) SqliteClose(database);
            throw new IOException(string.Format(Strings.DatabaseOpenFailed, path, error));
        }
        return new NativeSqlite(database);
    }

    /// <summary>
    /// Prepares a query and binds any conversation identifier as UTF-8 text.
    /// </summary>
    /// <param name="sql">A single SQLite query.</param>
    /// <param name="parameter">The optional value for the first query parameter.</param>
    /// <returns>A statement that owns the prepared SQLite handle.</returns>
    public SqliteStatement Prepare(string sql, string? parameter = null)
    {
        ThrowIfDisposed();
        int result = SqlitePrepare(database, sql, -1, out IntPtr statement, IntPtr.Zero);
        if (result != 0) throw new InvalidDataException(ErrorText(database));

        if (parameter != null)
        {
            result = SqliteBindText(statement, 1, parameter, -1, new IntPtr(SqliteTransient));
            if (result != 0)
            {
                SqliteFinalize(statement);
                throw new InvalidDataException(ErrorText(database));
            }
        }
        return new SqliteStatement(database, statement);
    }

    /// <summary>
    /// Releases the read-only connection after its statements have been disposed.
    /// </summary>
    public void Dispose()
    {
        if (disposed) return;
        SqliteClose(database);
        disposed = true;
    }

    static string ErrorText(IntPtr database) => Marshal.PtrToStringUTF8(SqliteErrorMessage(database)) ?? Strings.SqliteUnknownError;

    void ThrowIfDisposed()
    {
        if (disposed) throw new ObjectDisposedException(nameof(NativeSqlite));
    }

    [DllImport("winsqlite3.dll", EntryPoint = "sqlite3_open_v2", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    static extern int SqliteOpen([MarshalAs(UnmanagedType.LPUTF8Str)] string path, out IntPtr database, int flags, IntPtr vfs);

    [DllImport("winsqlite3.dll", EntryPoint = "sqlite3_prepare_v2", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    static extern int SqlitePrepare(IntPtr database, [MarshalAs(UnmanagedType.LPUTF8Str)] string sql, int byteCount, out IntPtr statement, IntPtr tail);

    [DllImport("winsqlite3.dll", EntryPoint = "sqlite3_bind_text", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    static extern int SqliteBindText(IntPtr statement, int index, [MarshalAs(UnmanagedType.LPUTF8Str)] string value, int byteCount, IntPtr destructor);

    [DllImport("winsqlite3.dll", EntryPoint = "sqlite3_step", CallingConvention = CallingConvention.Cdecl)]
    static extern int SqliteStep(IntPtr statement);

    [DllImport("winsqlite3.dll", EntryPoint = "sqlite3_column_text", CallingConvention = CallingConvention.Cdecl)]
    static extern IntPtr SqliteColumnText(IntPtr statement, int column);

    [DllImport("winsqlite3.dll", EntryPoint = "sqlite3_column_blob", CallingConvention = CallingConvention.Cdecl)]
    static extern IntPtr SqliteColumnBlob(IntPtr statement, int column);

    [DllImport("winsqlite3.dll", EntryPoint = "sqlite3_column_bytes", CallingConvention = CallingConvention.Cdecl)]
    static extern int SqliteColumnBytes(IntPtr statement, int column);

    [DllImport("winsqlite3.dll", EntryPoint = "sqlite3_column_int64", CallingConvention = CallingConvention.Cdecl)]
    static extern long SqliteColumnInt64(IntPtr statement, int column);

    [DllImport("winsqlite3.dll", EntryPoint = "sqlite3_column_type", CallingConvention = CallingConvention.Cdecl)]
    static extern int SqliteColumnType(IntPtr statement, int column);

    [DllImport("winsqlite3.dll", EntryPoint = "sqlite3_errmsg", CallingConvention = CallingConvention.Cdecl)]
    static extern IntPtr SqliteErrorMessage(IntPtr database);

    [DllImport("winsqlite3.dll", EntryPoint = "sqlite3_finalize", CallingConvention = CallingConvention.Cdecl)]
    static extern int SqliteFinalize(IntPtr statement);

    [DllImport("winsqlite3.dll", EntryPoint = "sqlite3_close", CallingConvention = CallingConvention.Cdecl)]
    static extern int SqliteClose(IntPtr database);

    /// <summary>
    /// Steps through one prepared query and reads its values as UTF-8 text, blobs or integers.
    /// </summary>
    public sealed class SqliteStatement : IDisposable
    {
        readonly IntPtr database;
        readonly IntPtr statement;
        bool statementDisposed;

        internal SqliteStatement(IntPtr database, IntPtr statement)
        {
            this.database = database;
            this.statement = statement;
        }

        /// <summary>Advances to the next query row, returning false at the end.</summary>
        public bool Read()
        {
            if (statementDisposed) throw new ObjectDisposedException(nameof(SqliteStatement));
            int result = SqliteStep(statement);
            if (result == ResultRow) return true;
            if (result == ResultDone) return false;
            throw new InvalidDataException(ErrorText(database));
        }

        /// <summary>Reads a column as UTF-8 text, returning an empty string for SQL null.</summary>
        public string GetText(int column)
        {
            if (SqliteColumnType(statement, column) == 5) return string.Empty;
            IntPtr value = SqliteColumnText(statement, column);
            int length = SqliteColumnBytes(statement, column);
            return value == IntPtr.Zero || length == 0 ? string.Empty : Marshal.PtrToStringUTF8(value, length) ?? string.Empty;
        }

        /// <summary>Reads a column as an integer.</summary>
        public long GetInt64(int column) => SqliteColumnInt64(statement, column);

        /// <summary>Reads a column as a byte sequence.</summary>
        public byte[] GetBlob(int column)
        {
            int length = SqliteColumnBytes(statement, column);
            if (length == 0) return [];
            byte[] value = new byte[length];
            Marshal.Copy(SqliteColumnBlob(statement, column), value, 0, length);
            return value;
        }

        /// <summary>Releases the prepared query.</summary>
        public void Dispose()
        {
            if (statementDisposed) return;
            SqliteFinalize(statement);
            statementDisposed = true;
        }
    }
}
