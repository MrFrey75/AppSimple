using AppSimple.AdminCli.Services;

namespace AppSimple.AdminCli.UI;

/// <summary>
/// Provides static helpers for rendering the Admin CLI user interface using standard
/// <see cref="Console"/> APIs and <see cref="ConsoleColor"/> — no third-party libraries.
/// </summary>
public static class ConsoleUI
{
    private const string AppTitle = "AppSimple Admin CLI";
    private const int Width = 72;

    // ─── Core rendering ────────────────────────────────────────────────────

    /// <summary>Clears the screen and optionally redraws the application header.</summary>
    /// <param name="showHeader">When <c>true</c> (default), the banner is redrawn.</param>
    public static void Clear(bool showHeader = true)
    {
        try { Console.Clear(); }
        catch (IOException) { /* Non-interactive environment (CI, tests) */ }
        if (showHeader) WriteHeader();
    }

    /// <summary>Writes the application banner at the top of the screen.</summary>
    public static void WriteHeader()
    {
        string border  = new('═', Width - 2);
        WriteColor($"╔{border}╗", ConsoleColor.DarkMagenta);
        string padded  = AppTitle.PadLeft((Width + AppTitle.Length) / 2).PadRight(Width - 2);
        WriteColor($"║{padded}║", ConsoleColor.DarkMagenta);
        WriteColor($"╚{border}╝", ConsoleColor.DarkMagenta);
        Console.WriteLine();
    }

    /// <summary>Writes a section heading with a coloured underline separator.</summary>
    /// <param name="heading">The heading text.</param>
    public static void WriteHeading(string heading)
    {
        WriteColor($"  {heading}", ConsoleColor.Magenta);
        WriteColor("  " + new string('─', heading.Length), ConsoleColor.DarkGray);
        Console.WriteLine();
    }

    /// <summary>Writes a full-width horizontal separator line.</summary>
    public static void WriteSeparator()
    {
        WriteColor(new string('─', Width), ConsoleColor.DarkGray);
    }

    /// <summary>Writes a blank line.</summary>
    public static void WriteLine() => Console.WriteLine();

    // ─── Status messages ───────────────────────────────────────────────────

    /// <summary>Writes a success message in green.</summary>
    public static void WriteSuccess(string message) =>
        WriteColor($"  ✓  {message}", ConsoleColor.Green);

    /// <summary>Writes an error message in red.</summary>
    public static void WriteError(string message) =>
        WriteColor($"  ✗  {message}", ConsoleColor.Red);

    /// <summary>Writes a warning message in yellow.</summary>
    public static void WriteWarning(string message) =>
        WriteColor($"  ⚠  {message}", ConsoleColor.Yellow);

    /// <summary>Writes an informational message in cyan.</summary>
    public static void WriteInfo(string message) =>
        WriteColor($"  ℹ  {message}", ConsoleColor.Cyan);

    /// <summary>Writes a labelled key/value pair, used for status display.</summary>
    /// <param name="key">The label.</param>
    /// <param name="value">The value.</param>
    public static void WriteKeyValue(string key, string value)
    {
        Console.Write("  ");
        WriteColor($"{key,-20}", ConsoleColor.DarkCyan, newLine: false);
        WriteColor(value, ConsoleColor.White);
    }

    // ─── Menu rendering ────────────────────────────────────────────────────

    /// <summary>Writes a numbered menu item.</summary>
    /// <param name="number">The selection number shown to the user.</param>
    /// <param name="label">The menu item label.</param>
    /// <param name="description">Optional short description shown in dim text.</param>
    public static void WriteMenuItem(int number, string label, string? description = null)
    {
        Console.Write("  ");
        WriteColor($"[{number}]", ConsoleColor.DarkMagenta, newLine: false);
        Console.Write($" {label}");
        if (description is not null)
            WriteColor($"  — {description}", ConsoleColor.DarkGray);
        else
            Console.WriteLine();
    }

    /// <summary>Writes the exit/back menu item (always item 0).</summary>
    /// <param name="label">Label override; defaults to "Back".</param>
    public static void WriteBackItem(string label = "Back")
    {
        Console.WriteLine();
        Console.Write("  ");
        WriteColor("[0]", ConsoleColor.DarkGray, newLine: false);
        WriteColor($" {label}", ConsoleColor.DarkGray);
        Console.WriteLine();
    }

    // ─── Input helpers ─────────────────────────────────────────────────────

    /// <summary>
    /// Reads a required string from the user with a coloured prompt.
    /// Repeats until a non-empty value is entered.
    /// </summary>
    /// <param name="prompt">The prompt text.</param>
    public static string ReadLine(string prompt)
    {
        while (true)
        {
            Console.Write("  ");
            WriteColor($"{prompt}: ", ConsoleColor.White, newLine: false);
            string? input = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(input)) return input;
            WriteError("This field is required.");
        }
    }

    /// <summary>
    /// Reads an optional string from the user. Returns <paramref name="current"/> if the user
    /// presses Enter without typing anything.
    /// </summary>
    /// <param name="prompt">The prompt text.</param>
    /// <param name="current">The current value displayed as a hint.</param>
    public static string? ReadOptionalLine(string prompt, string? current = null)
    {
        string hint = current is not null ? $" (current: {current})" : " (leave blank to skip)";
        Console.Write("  ");
        WriteColor($"{prompt}{hint}: ", ConsoleColor.White, newLine: false);
        string? input = Console.ReadLine()?.Trim();
        return string.IsNullOrEmpty(input) ? current : input;
    }

    /// <summary>
    /// Reads a password from the console, masking each character with an asterisk.
    /// </summary>
    /// <param name="prompt">The prompt text.</param>
    public static string ReadPassword(string prompt = "Password")
    {
        Console.Write("  ");
        WriteColor($"{prompt}: ", ConsoleColor.White, newLine: false);

        if (Console.IsInputRedirected)
        {
            var line = Console.ReadLine() ?? string.Empty;
            Console.WriteLine();
            return line;
        }

        var sb = new System.Text.StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }
            if (key.Key == ConsoleKey.Backspace)
            {
                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                    Console.Write("\b \b");
                }
            }
            else if (key.KeyChar != '\0')
            {
                sb.Append(key.KeyChar);
                Console.Write('*');
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Reads a valid menu selection between 0 and <paramref name="max"/> (inclusive).
    /// </summary>
    /// <param name="max">The highest valid selection number.</param>
    public static int ReadMenuChoice(int max)
    {
        while (true)
        {
            Console.Write("  ");
            WriteColor("Select: ", ConsoleColor.White, newLine: false);
            string? input = Console.ReadLine()?.Trim();
            if (int.TryParse(input, out int choice) && choice >= 0 && choice <= max)
                return choice;
            WriteError($"Please enter a number between 0 and {max}.");
        }
    }

    /// <summary>Asks a yes/no confirmation question. Returns <c>true</c> for yes.</summary>
    /// <param name="question">The question to display.</param>
    public static bool Confirm(string question)
    {
        Console.Write("  ");
        WriteColor($"{question} [y/N]: ", ConsoleColor.Yellow, newLine: false);
        string? answer = Console.ReadLine()?.Trim().ToLower();
        return answer == "y" || answer == "yes";
    }

    /// <summary>Pauses execution until the user presses any key.</summary>
    /// <param name="message">The message to display.</param>
    public static void Pause(string message = "Press any key to continue...")
    {
        Console.WriteLine();
        WriteColor($"  {message}", ConsoleColor.DarkGray);

        if (Console.IsInputRedirected)
        {
            Console.ReadLine();
            return;
        }

        Console.ReadKey(intercept: true);
    }

    // ─── Data display ──────────────────────────────────────────────────────

    /// <summary>Displays a formatted table of users.</summary>
    /// <param name="users">The list of users to display.</param>
    public static void WriteUserTable(IEnumerable<UserDto> users)
    {
        var list = users.ToList();
        if (list.Count == 0)
        {
            WriteInfo("No users found.");
            return;
        }

        string header = $"  {"#",-4} {"Username",-20} {"Email",-28} {"Role",-8} {"Active",-6}";
        WriteColor(header, ConsoleColor.DarkMagenta);
        WriteColor("  " + new string('─', header.Length - 2), ConsoleColor.DarkGray);

        for (int i = 0; i < list.Count; i++)
        {
            var u = list[i];
            string roleLabel   = u.Role == 1 ? "Admin" : "User";
            string activeLabel = u.IsActive ? "Yes" : "No";
            ConsoleColor rowColor = u.IsSystem ? ConsoleColor.DarkYellow : ConsoleColor.White;
            string sysTag = u.IsSystem ? " ⚙" : "";
            WriteColor(
                $"  {i + 1,-4} {u.Username + sysTag,-20} {u.Email,-28} {roleLabel,-8} {activeLabel,-6}",
                rowColor);
        }
        Console.WriteLine();
    }

    /// <summary>Displays the full detail view of a single user.</summary>
    /// <param name="user">The user to display.</param>
    public static void WriteUserDetail(UserDto user)
    {
        void Row(string label, string? value)
        {
            Console.Write("  ");
            WriteColor($"{label,-18}", ConsoleColor.DarkMagenta, newLine: false);
            WriteColor(value ?? "—", ConsoleColor.White);
        }

        WriteSeparator();
        Row("Uid",          user.Uid.ToString());
        Row("Username",     user.Username);
        Row("Email",        user.Email);
        Row("Full Name",    user.FullName ?? "—");
        Row("Phone",        user.PhoneNumber);
        Row("Date of Birth",user.DateOfBirth?.ToString("yyyy-MM-dd"));
        Row("Bio",          user.Bio);
        Row("Role",         user.Role == 1 ? "Admin" : "User");
        Row("Active",       user.IsActive ? "Yes" : "No");
        Row("System",       user.IsSystem ? "Yes" : "No");
        Row("Created At",   user.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
        WriteSeparator();
        Console.WriteLine();
    }

    // ─── Private helpers ───────────────────────────────────────────────────

    private static void WriteColor(string text, ConsoleColor color, bool newLine = true)
    {
        if (!Console.IsOutputRedirected) Console.ForegroundColor = color;
        if (newLine) Console.WriteLine(text);
        else Console.Write(text);
        if (!Console.IsOutputRedirected) Console.ResetColor();
    }
}
