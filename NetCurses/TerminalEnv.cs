using NetCurses.Models;
using Mindmagma.Curses;
using System.Diagnostics;
using System.Text;

namespace NetCurses;
public static class TerminalEnv
{
    public static string ExitKey = "esc";

    private static IntPtr _screen = IntPtr.Zero;
    private static List<int>? _initializedColorPairIds = null;
    private static NcWindow? _rootWindow = null;

    internal static bool NCursesInitialized => _screen != IntPtr.Zero;
    internal static List<int>? InitializedColorPairIds => _initializedColorPairIds;

    public static void Execute<T>() where T : NcWindow, new()
    {
        InitNcurses();

        _rootWindow = new T();
        _rootWindow.Initialize(WindowSize.Fullscreen);
        _rootWindow.IsActive = true;
        _rootWindow.Update(null);

        string? keyPressed;
        do
        {
            var keyCode = NCurses.GetChar();
            keyPressed = GetKeyboardString(keyCode);

            if (keyPressed is not null) _rootWindow.Update(keyPressed);
        }
        while (keyPressed != ExitKey);

        _rootWindow.Dispose();
        CloseNcurses();
    }

    /// <summary>
    /// Initializes the color pairs (if not already initialized)
    /// </summary>
    public static void InitColors(IEnumerable<ColorPair> colors)
    {
        InitNcurses();
        if (!NCurses.HasColors())
        {
            Debug.WriteLine("Terminal does not support colors");
            return;
        }

        if (_initializedColorPairIds is not null) return;
        _initializedColorPairIds = new();

        NCurses.StartColor();
        foreach (var c in colors)
        {
            NCurses.InitPair((short)c.Id, c.Foreground, c.Background);
            _initializedColorPairIds.Add(c.Id);
        }
    }

    private static void InitNcurses()
    {
        if (NCursesInitialized) return;

        _screen = NCurses.InitScreen();
        NCurses.Raw();
        NCurses.NoEcho();
        NCurses.CBreak();
        NCurses.Keypad(_screen, true);
        NCurses.NoDelay(_screen, true);
        NCurses.Refresh();
    }

    private static void CloseNcurses()
    {
        if (!NCursesInitialized) return;

        NCurses.EndWin();
        _screen = IntPtr.Zero;
    }

    private static string? GetKeyboardString(int keyCode)
    {
        if (keyCode == -1)
        {
            return null;
        }

        if (keyCode > 32 && keyCode < 127)
        {
            // visible characters (letter, numbers, symbols)
            return Encoding.ASCII.GetString(new byte[] { (byte)keyCode });
        }

        var keyStr = keyCode switch
        {
            8 => "backspace",
            9 => "tab",
            10 => "enter",
            13 => "enter",
            27 => "esc",
            32 => "space",
            127 => "del",
            258 => "down",
            259 => "up",
            260 => "left",
            261 => "right",
            263 => "backspace",
            269 => "f5",
            330 => "del",
            _ => null
        };

        if (keyStr is null)
        {
            Debug.WriteLine($"Failed to assign string value to keycode [{keyCode}]");
        }

        return keyStr;
    }
}
