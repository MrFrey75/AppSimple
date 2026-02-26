using AppSimple.Core.Models;

namespace AppSimple.UserCLI.UI;

/// <summary>
/// Provides static helpers for rendering the CLI user interface using standard
/// <see cref="Console"/> APIs and <see cref="ConsoleColor"/> — no third-party libraries.
/// </summary>
public static class ConsoleUI
{
    private const string AppTitle = "AppSimple User CLI";
    private const int Width = 70;

    // ─── Core rendering ────────────────────────────────────────────────────

    /// <summary>Clears the screen and optionally redraws the application header.</summary>
    public static void Clear(bool showHeader = true)
    {
        try { Console.Clear(); }
        catch (IOException) { /* Non-interactive environment (CI, tests) */ }
        if (showHeader) WriteHeader();
    }

    /// <summary>Writes the application banner at the top of the screen.</summary>
    public static void WriteHeader()
    {
        string border = new('═', Width - 2);
        WriteColor($"╔{border}╗", ConsoleColor.DarkCyan);
        string padded = AppTitle.PadLeft((Width + AppTitle.Length) / 2).PadRight(Width - 2);
        WriteColor($"║{padded}║", ConsoleColor.DarkCyan);
        WriteColor($"╚{border}╝", ConsoleColor.DarkCyan);
        Console.WriteLine();
    }

    /// <summary>Writes a section heading with a coloured underline separator.</summary>
    /// <param name="heading">The heading text.</param>
    public static void WriteHeading(string heading)
    {
        WriteColor($"  {heading}", ConsoleColor.Cyan);
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

    // ─── Menu rendering ────────────────────────────────────────────────────

    /// <summary>Writes a numbered menu item.</summary>
    /// <param name="number">The selection number shown to the user.</param>
    /// <param name="label">The menu item label.</param>
    /// <param name="description">Optional short description shown in dim text.</param>
    public static void WriteMenuItem(int number, string label, string? description = null)
    {
        Console.Write("  ");
        WriteColor($"[{number}]", ConsoleColor.DarkCyan);
        Console.Write($" {label}");
        if (description is not null)
        {
            WriteColor($"  — {description}", ConsoleColor.DarkGray);
        }
        else
        {
            Console.WriteLine();
        }
    }

    /// <summary>Writes a visual group separator with a label inside the menu list.</summary>
    public static void WriteMenuGroupLabel(string label)
    {
        Console.WriteLine();
        WriteColor($"  ── {label} ──", ConsoleColor.DarkGray);
    }

    /// <summary>Writes the exit/back menu item (always item 0).</summary>
    /// <param name="label">Label override; defaults to "Back".</param>
    public static void WriteBackItem(string label = "Back")
    {
        Console.WriteLine();
        Console.Write("  ");
        WriteColor("[0]", ConsoleColor.DarkGray);
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
    /// Reads an optional string from the user. Returns the existing value if the user
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

        // When stdin is redirected (CI, dotnet test) Console.ReadKey is unavailable.
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
    /// 0 is always accepted as "back/exit".
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

    /// <summary>
    /// Asks a yes/no confirmation question. Returns <c>true</c> for yes.
    /// </summary>
    /// <param name="question">The question to display.</param>
    public static bool Confirm(string question)
    {
        Console.Write("  ");
        WriteColor($"{question} [y/N]: ", ConsoleColor.Yellow, newLine: false);
        string? answer = Console.ReadLine()?.Trim().ToLower();
        return answer == "y" || answer == "yes";
    }

    /// <summary>Pauses execution until the user presses any key.</summary>
    /// <param name="message">The message to display. Defaults to a standard prompt.</param>
    public static void Pause(string message = "Press any key to continue...")
    {
        Console.WriteLine();
        WriteColor($"  {message}", ConsoleColor.DarkGray);

        // When stdin is redirected (CI, dotnet test) Console.ReadKey is unavailable.
        if (Console.IsInputRedirected)
        {
            Console.ReadLine();
            return;
        }

        Console.ReadKey(intercept: true);
    }

    // ─── Data display ──────────────────────────────────────────────────────

    /// <summary>Displays a formatted table of users.</summary>
    /// <param name="users">The users to display.</param>
    public static void WriteUserTable(IEnumerable<User> users)
    {
        var list = users.ToList();
        if (list.Count == 0)
        {
            WriteInfo("No users found.");
            return;
        }

        string header = $"  {"#",-4} {"Username",-20} {"Email",-28} {"Role",-8} {"Active",-6}";
        WriteColor(header, ConsoleColor.DarkCyan);
        WriteColor("  " + new string('─', header.Length - 2), ConsoleColor.DarkGray);

        for (int i = 0; i < list.Count; i++)
        {
            var u = list[i];
            string activeLabel = u.IsActive ? "Yes" : "No";
            ConsoleColor rowColor = u.IsSystem ? ConsoleColor.DarkYellow : ConsoleColor.White;
            string sysTag = u.IsSystem ? " ⚙" : "";
            WriteColor(
                $"  {i + 1,-4} {u.Username + sysTag,-20} {u.Email,-28} {u.Role,-8} {activeLabel,-6}",
                rowColor);
        }
        Console.WriteLine();
    }

    /// <summary>Displays the full detail view of a single user.</summary>
    /// <param name="user">The user to display.</param>
    public static void WriteUserDetail(User user)
    {
        void Row(string label, string? value)
        {
            Console.Write($"  ");
            WriteColor($"{label,-18}", ConsoleColor.DarkCyan, newLine: false);
            WriteColor(value ?? "—", ConsoleColor.White);
        }

        WriteSeparator();
        Row("Username",     user.Username);
        Row("Email",        user.Email);
        Row("Full Name",    user.FullName ?? "—");
        Row("Phone",        user.PhoneNumber);
        Row("Date of Birth",user.DateOfBirth?.ToString("yyyy-MM-dd"));
        Row("Bio",          user.Bio);
        Row("Role",         user.Role.ToString());
        Row("Active",       user.IsActive ? "Yes" : "No");
        Row("Member Since", user.CreatedAt.ToString("yyyy-MM-dd"));
        WriteSeparator();
        Console.WriteLine();
    }

    /// <summary>Displays a table of notes.</summary>
    public static void WriteNoteTable(IEnumerable<Note> notes)
    {
        var list = notes.ToList();
        string header = $"  {"#",-4} {"Title",-30} {"Tags",-20} {"Updated",-12}";
        WriteColor(header, ConsoleColor.DarkCyan);
        WriteColor("  " + new string('─', header.Length - 2), ConsoleColor.DarkGray);

        for (int i = 0; i < list.Count; i++)
        {
            var n = list[i];
            string title = string.IsNullOrEmpty(n.Title) ? "(untitled)" : n.Title;
            if (title.Length > 29) title = title[..26] + "...";
            string tags = n.Tags.Count == 0 ? "—" : string.Join(", ", n.Tags.Select(t => t.Name));
            if (tags.Length > 19) tags = tags[..16] + "...";
            WriteColor($"  {i + 1,-4} {title,-30} {tags,-20} {n.UpdatedAt:yyyy-MM-dd}", ConsoleColor.White);
        }
        Console.WriteLine();
    }

    /// <summary>Displays the full detail of a single note.</summary>
    public static void WriteNoteDetail(Note note)
    {
        void Row(string label, string? value)
        {
            Console.Write("  ");
            WriteColor($"{label,-14}", ConsoleColor.DarkCyan, newLine: false);
            WriteColor(value ?? "—", ConsoleColor.White);
        }
        WriteSeparator();
        Row("Title",   string.IsNullOrEmpty(note.Title) ? "(untitled)" : note.Title);
        Row("Tags",    note.Tags.Count == 0 ? "—" : string.Join(", ", note.Tags.Select(t => t.Name)));
        Row("Updated", note.UpdatedAt.ToString("yyyy-MM-dd HH:mm"));
        WriteSeparator();
        Console.WriteLine();
        WriteColor("  " + note.Content, ConsoleColor.White);
        Console.WriteLine();
    }

    /// <summary>Displays a table of tags.</summary>
    public static void WriteTagTable(IEnumerable<Tag> tags)
    {
        var list = tags.ToList();
        string header = $"  {"#",-4} {"Name",-20} {"Color",-10} {"Description",-30}";
        WriteColor(header, ConsoleColor.DarkCyan);
        WriteColor("  " + new string('─', header.Length - 2), ConsoleColor.DarkGray);

        for (int i = 0; i < list.Count; i++)
        {
            var t = list[i];
            string desc = t.Description ?? "";
            if (desc.Length > 29) desc = desc[..26] + "...";
            WriteColor($"  {i + 1,-4} {t.Name,-20} {t.Color ?? "",-10} {desc}", ConsoleColor.White);
        }
        Console.WriteLine();
    }

    /// <summary>Displays a table of contacts.</summary>
    public static void WriteContactTable(IEnumerable<Contact> contacts)
    {
        var list = contacts.ToList();
        string header = $"  {"#",-4} {"Name",-28} {"Primary Email",-30} {"Primary Phone",-18}";
        WriteColor(header, ConsoleColor.DarkCyan);
        WriteColor("  " + new string('─', header.Length - 2), ConsoleColor.DarkGray);

        for (int i = 0; i < list.Count; i++)
        {
            var c = list[i];
            string email = c.EmailAddresses.FirstOrDefault(e => e.IsPrimary)?.Email
                        ?? c.EmailAddresses.FirstOrDefault()?.Email ?? "—";
            string phone = c.PhoneNumbers.FirstOrDefault(p => p.IsPrimary)?.Number
                        ?? c.PhoneNumbers.FirstOrDefault()?.Number ?? "—";
            if (email.Length > 29) email = email[..26] + "...";
            WriteColor($"  {i + 1,-4} {c.Name,-28} {email,-30} {phone}", ConsoleColor.White);
        }
        Console.WriteLine();
    }

    /// <summary>Displays the full detail of a single contact.</summary>
    public static void WriteContactDetail(Contact contact)
    {
        void Row(string label, string? value)
        {
            Console.Write("  ");
            WriteColor($"{label,-16}", ConsoleColor.DarkCyan, newLine: false);
            WriteColor(value ?? "—", ConsoleColor.White);
        }
        WriteSeparator();
        Row("Name", contact.Name);

        if (contact.EmailAddresses.Count > 0)
        {
            foreach (var e in contact.EmailAddresses)
                Row(e.IsPrimary ? "Email ★" : "Email", $"{e.Email}  [{e.Type}]");
        }

        if (contact.PhoneNumbers.Count > 0)
        {
            foreach (var p in contact.PhoneNumbers)
                Row(p.IsPrimary ? "Phone ★" : "Phone", $"{p.Number}  [{p.Type}]");
        }

        if (contact.Addresses.Count > 0)
        {
            foreach (var a in contact.Addresses)
            {
                string addr = $"{a.Street}, {a.City}";
                if (!string.IsNullOrEmpty(a.State))      addr += $", {a.State}";
                if (!string.IsNullOrEmpty(a.PostalCode)) addr += $" {a.PostalCode}";
                addr += $", {a.Country}  [{a.Type}]";
                Row(a.IsPrimary ? "Address ★" : "Address", addr);
            }
        }

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
