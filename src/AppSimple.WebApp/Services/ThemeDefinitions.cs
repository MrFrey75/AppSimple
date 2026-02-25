namespace AppSimple.WebApp.Services;

/// <summary>Provides CSS variable definitions for all supported themes.</summary>
public static class ThemeDefinitions
{
    /// <summary>All available theme names.</summary>
    public static readonly IReadOnlyList<string> Names =
    [
        "CatppuccinMocha",
        "CatppuccinLatte",
        "Dracula",
        "Nord",
        "SolarizedLight"
    ];

    /// <summary>Display labels matching <see cref="Names"/> order.</summary>
    public static readonly IReadOnlyList<string> Labels =
    [
        "Catppuccin Mocha",
        "Catppuccin Latte",
        "Dracula",
        "Nord",
        "Solarized Light"
    ];

    private static readonly Dictionary<string, string> _css = new()
    {
        ["CatppuccinMocha"] = """
            --bg:#1e1e2e;--surface:#181825;--overlay:#313244;
            --text:#cdd6f4;--subtext:#a6adc8;--accent:#89b4fa;
            --green:#a6e3a1;--red:#f38ba8;--yellow:#f9e2af;
            --border:#45475a;--navbar:#11111b;
            """,
        ["CatppuccinLatte"] = """
            --bg:#eff1f5;--surface:#e6e9ef;--overlay:#ccd0da;
            --text:#4c4f69;--subtext:#6c6f85;--accent:#1e66f5;
            --green:#40a02b;--red:#d20f39;--yellow:#df8e1d;
            --border:#bcc0cc;--navbar:#dce0e8;
            """,
        ["Dracula"] = """
            --bg:#282a36;--surface:#1e1f29;--overlay:#44475a;
            --text:#f8f8f2;--subtext:#6272a4;--accent:#bd93f9;
            --green:#50fa7b;--red:#ff5555;--yellow:#f1fa8c;
            --border:#44475a;--navbar:#191a21;
            """,
        ["Nord"] = """
            --bg:#2e3440;--surface:#242933;--overlay:#3b4252;
            --text:#eceff4;--subtext:#d8dee9;--accent:#88c0d0;
            --green:#a3be8c;--red:#bf616a;--yellow:#ebcb8b;
            --border:#4c566a;--navbar:#1c2130;
            """,
        ["SolarizedLight"] = """
            --bg:#fdf6e3;--surface:#eee8d5;--overlay:#93a1a1;
            --text:#657b83;--subtext:#839496;--accent:#268bd2;
            --green:#859900;--red:#dc322f;--yellow:#b58900;
            --border:#93a1a1;--navbar:#e8e1cc;
            """
    };

    /// <summary>Returns the CSS variable block for the given theme name, falling back to CatppuccinMocha.</summary>
    public static string GetCss(string themeName) =>
        _css.TryGetValue(themeName, out var css) ? css : _css["CatppuccinMocha"];
}
