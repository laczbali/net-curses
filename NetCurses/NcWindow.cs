using NetCurses.Models;
using Mindmagma.Curses;

namespace NetCurses;

public abstract partial class NcWindow
{
    public bool IsActive = false;

    private IntPtr _windowObj = IntPtr.Zero;

    private WindowSize _size = WindowSize.Null;

    private string _id = GetId();
    public string Id => _id;

    protected bool isInitialized => _windowObj != IntPtr.Zero;
    public WindowSize Size => _size;
    public List<NcWindow> Children = new();

    /// <summary>
    /// Run any necessary startup code here.
    /// </summary>
    /// <param name="softInit">If set to true, we are just reloading. Otherwise it is a fresh start.</param>
    protected virtual void InitializeInner() { }
    /// <summary>
    /// Run the core logic of the window here
    /// </summary>
    protected virtual UpdateResult UpdateInner(string? keypressed) { return new UpdateResult(); }
    /// <summary>
    /// Run any necessary cleanup code here.
    /// </summary>
    /// <param name="softDispose">If set to true, we are just reloading. Otherwise we are stopping for good.</param>
    protected virtual void DisposeInner() { }

    /// <summary>
    /// Creates and applies default config to a new window, <br/>
    /// then calls InitializeInner
    /// </summary>
    public void Initialize(WindowSize size)
    {
        if (isInitialized) return;

        _size = size;
        _windowObj = NCurses.NewWindow(size.Rows, size.Columns, size.RowOrigin, size.ColumnsOrigin);
        NCurses.Keypad(_windowObj, true);
        NCurses.NoDelay(_windowObj, true);

        InitializeInner();
    }

    /// <summary>
    /// Calls UpdateInner, refresh, then calls Update on all children
    /// </summary>
    public UpdateResult Update(string? keypressed)
    {
        var result = UpdateInner(keypressed);
        if (result.RefreshWindow) NCurses.WindowRefresh(_windowObj);

        var childUpdateResults = Children?.Select(x => x?.Update(keypressed)).ToList() ?? new();
        childUpdateResults.Where(x => x?.RemoveSelfFromParent == true).ToList().ForEach(x =>
        {
            var child = Children?.Find(c => c.Id == x?.WindowId);
            if (child is null) return;
            child.Dispose();
            Children?.Remove(child);
        });

        return new UpdateResult
        {
            WindowId = Id,
            RemoveSelfFromParent = result.RemoveSelfFromParent
        };
    }

    /// <summary>
    /// 1. Calls Dispose on all children    <br/>
    /// 2. Calls virtual DisposeInner       <br/>
    /// 3. Deletes the window object        <br/>
    /// </summary>
    public void Dispose()
    {
        Children?.ForEach(x => x?.Dispose());
        DisposeInner();
        NCurses.DeleteWindow(_windowObj);
        _windowObj = IntPtr.Zero;
    }

    private static string GetId()
    {
        const string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(allowedChars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public override bool Equals(object? obj)
    {
        return Id == (obj as NcWindow)?.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
