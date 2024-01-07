using NetCurses.Models;
using Mindmagma.Curses;

namespace NetCurses;
public abstract partial class NcWindow
{
    /// <summary>
    /// Move cursor to the specified position, and write the string
    /// </summary>
    public void WriteAtPosition(int y, int x, string str)
        => NCurses.MoveWindowAddString(_windowObj, y, x, str);

    /// <summary>
    /// Write the string at the current cursor position
    /// </summary>
    public void Write(string str)
        => NCurses.WindowAddString(_windowObj, str);

    /// <summary>
    /// Move the cursor to the specified position.
    /// If a dimension is null, the cursor will not be moved in that dimension.
    /// </summary>
    public void MoveCursor(int? y = null, int? x = null)
    {
        NCurses.GetYX(_windowObj, out var curY, out var curX);
        NCurses.WindowMove(_windowObj, y ?? curY, x ?? curX);
    }

    /// <summary>
    /// Toggle a CursesAttribute on or off
    /// </summary>
    public void ToggleAttribute(uint attribute, bool on)
    {
        if (on)
        {
            NCurses.WindowAttributeOn(_windowObj, attribute);
        }
        else
        {
            NCurses.WindowAttributeOff(_windowObj, attribute);
        }
    }

    /// <summary>
    /// Draws the window border and title
    /// </summary>
    public void ToggleBox(string? title = null, bool on = true)
    {
        if (on)
        {
            NCurses.WindowBorder(_windowObj, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0);
            if (title is not null)
            {
                NCurses.GetMaxYX(_windowObj, out _, out var colCount);
                colCount -= 6; // 3 chars on each side (-- title --)
                if (title.Length > colCount)
                {
                    title = title.Substring(0, colCount);
                }
                NCurses.MoveWindowAddString(_windowObj, 0, 2, $" {title} ");
            }
        }
        else
        {
            NCurses.WindowBorder(_windowObj, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');
        }
    }

    /// <summary>
    /// Toggles the color pair on or off
    /// </summary>
    public void ToggleColorPair(int pairId, bool on)
    {
        if (TerminalEnv.InitializedColorPairIds is null)
        {
            throw new InvalidOperationException("Color pairs have not been initialized");
        }
        if (!TerminalEnv.InitializedColorPairIds.Contains(pairId))
        {
            throw new ArgumentException($"Color pair {pairId} has not been initialized");
        }

        if (on)
        {
            NCurses.WindowAttributeOn(_windowObj, NCurses.ColorPair(pairId));
        }
        else
        {
            NCurses.WindowAttributeOff(_windowObj, NCurses.ColorPair(pairId));
        }
    }

    /// <inheritdoc cref="ToggleColorPair(NcWindow, int, bool)"/>
    public void ToggleColorPair(ColorPair color, bool on)
        => ToggleColorPair(color.Id, on);

    /// <summary>
    /// Starts at the current cursor position and writes ' ' until the end of the line
    /// </summary>
    /// <param name="rightMargin">It will leave this many characters untouched at the end of the line</param>
    public void ClearRestOfLine(int rightMargin = 1)
    {
        NCurses.GetYX(_windowObj, out var rowCursor, out var colCursor);
        NCurses.GetMaxYX(_windowObj, out var maxRow, out var maxCol);
        maxCol -= rightMargin;

        for (var i = colCursor; i < maxCol; i++)
        {
            NCurses.WindowAddChar(_windowObj, ' ');
        }
    }
}
