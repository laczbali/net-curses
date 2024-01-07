namespace NetCurses.Models;
public class UpdateResult
{
    public string WindowId { get; set; } = string.Empty;
    public bool RefreshWindow { get; set; } = true;
    public bool RemoveSelfFromParent { get; set; } = false;
}
