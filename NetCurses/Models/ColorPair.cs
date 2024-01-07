namespace NetCurses.Models;

/// <summary>
/// It is recommended to use the CursesColor enum for the Foreground and Background properties.
/// The Id should start at 1.
/// </summary>
public class ColorPair
{
    public int Id { get; set; }
    public short Foreground { get; set; }
    public short Background { get; set; }
}
