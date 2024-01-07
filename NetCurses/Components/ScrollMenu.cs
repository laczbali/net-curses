using NetCurses.Extensions;
using Mindmagma.Curses;

namespace NetCurses.Components;

/// <summary>
/// Set the menu items by setting <see cref="Items"/>.      <br/>
/// Display the menu by calling <see cref="DisplayMenu"/>.  <br/>
/// Move the cursor by calling <see cref="StepCursor"/>.    <br/>
/// <see cref="ResetMenu"/> should be called when the menu items change. <br/>
/// <br/>
/// The behaviour of the menu can be customized
/// by overriding the virtual properties
/// and the virtual methods.
/// </summary>
public abstract class ScrollMenu : NcWindow
{
    // settings
    protected virtual int CursorLag => 5;
    protected virtual bool HaveBorders => true;
    protected virtual bool ShowScrollBar => true;
    protected virtual int LeftMargin => 1;
    protected abstract IEnumerable<string> Items { get; }

    // settings (derived)
    private int _rightMargin => 1 + (ShowScrollBar ? 2 : 0); // 1ch right margin is needed, otherwise things freak out around the last row
    private int _xOffset => (HaveBorders ? 1 : 0) + LeftMargin;
    private int _maxRowDisplayLength => Size.Columns - _xOffset - _rightMargin;
    private int _maxRowDisplayCount => Size.Rows - (HaveBorders ? 2 : 0);
    private int _maxRowCount => Math.Min(_maxRowDisplayCount, Items.Count());
    private int _maxItemOffset => Items.Count() - _maxRowCount;
    private int _scrollBarHeight
    {
        get
        {
            var rowMultiplier = (double)_maxRowCount / Items.Count();
            var barHeight = _maxRowCount * rowMultiplier;
            if (barHeight > _maxRowCount) barHeight = _maxRowCount;
            if (barHeight < 1) barHeight = 1;
            return (int)barHeight;
        }
    }


    // current state
    protected int CursorItemIndex { get; private set; } = 0;

    private int _itemOffset = 0;

    // current state (derived)
    private int _firstDisplayedItemIndex => _itemOffset;
    private int _lastDisplayedItemIndex => _itemOffset + _maxRowCount - 1;
    private int _scrollBarOffset => (int)((double)_itemOffset / _maxItemOffset * (_maxRowCount - _scrollBarHeight));

    // state change
    protected void ResetMenu()
    {
        CursorItemIndex = 0;
        _itemOffset = 0;
    }

    protected void StepCursor(string direction)
    {
        if (direction != "up" && direction != "down")
            throw new ArgumentException("direction must be 'up' or 'down'");
        StepCursor(direction == "up" ? -1 : 1);
    }

    protected void StepCursor(int step)
    {
        CursorItemIndex += step;

        // clamp cursor to list items, but allow wrapping
        if (CursorItemIndex < 0) CursorItemIndex = Items.Count() - 1;
        if (CursorItemIndex >= Items.Count()) CursorItemIndex = 0;

        // if the cursor is not on the screen, adjust the offset so it will be
        if (CursorItemIndex < _firstDisplayedItemIndex) _itemOffset = CursorItemIndex;
        if (CursorItemIndex > _lastDisplayedItemIndex) _itemOffset = CursorItemIndex - _maxRowCount + 1;

        // handle cursor lag, if needed
        if (step > 0)
        {
            var lagBuffer = _lastDisplayedItemIndex - CursorItemIndex;
            if (lagBuffer < CursorLag) _itemOffset += CursorLag - lagBuffer;
            if (_itemOffset > Items.Count() - _maxRowCount) _itemOffset = Items.Count() - _maxRowCount;
        }
        else
        {
            var lagBuffer = CursorItemIndex - _firstDisplayedItemIndex;
            if (lagBuffer < CursorLag) _itemOffset -= CursorLag - lagBuffer;
            if (_itemOffset < 0) _itemOffset = 0;
        }
    }

    // display logic
    protected void DisplayMenu()
    {
        var itemIndex = _itemOffset;
        for (var lineIndex = 0; lineIndex < _maxRowCount; lineIndex++)
        {
            DisplayRow(itemIndex, lineIndex);
            itemIndex++;
        }

        for (var i = _maxRowCount; i < _maxRowDisplayCount; i++)
        {
            var yOffset = (HaveBorders ? 1 : 0) + i;
            this.MoveCursor(yOffset, _xOffset);
            this.ClearRestOfLine(_rightMargin - (ShowScrollBar ? 2 : 0));
        }
    }

    private void DisplayRow(int itemIndex, int lineIndex)
    {
        var item = Items.ElementAt(itemIndex).ShortenString(_maxRowDisplayLength);
        var yOffset = (HaveBorders ? 1 : 0) + lineIndex;

        PreDisplayRow(itemIndex, lineIndex);
        this.WriteAtPosition(yOffset, _xOffset, item);
        this.ClearRestOfLine(_rightMargin);
        PostDisplayRow(itemIndex, lineIndex);

        DrawScrollBar(lineIndex);
    }

    private void DrawScrollBar(int lineIndex)
    {
        if (!ShowScrollBar) return;

        var writeAtLine = true;
        if (lineIndex < _scrollBarOffset) writeAtLine = false;
        if (lineIndex > _scrollBarOffset + _scrollBarHeight - 1) writeAtLine = false;

        if (writeAtLine)
        {
            this.Write(" #");
        }
        else
        {
            this.ClearRestOfLine(_rightMargin - (ShowScrollBar ? 2 : 0));
        }
    }

    // display logic (virtual)

    /// <summary>
    /// Runs before the current row is displayed.
    /// By default, it sets the STANDOUT/BOLD if the cursor is on the current row.
    /// </summary>
    /// <param name="itemIndex">Which item we are currently showing</param>
    /// <param name="lineIndex">Which window line we are currently on</param>
    protected virtual void PreDisplayRow(int itemIndex, int lineIndex)
    {
        this.ToggleAttribute(
            CursesAttribute.STANDOUT,
            itemIndex == CursorItemIndex && IsActive);
        this.ToggleAttribute(
            CursesAttribute.BOLD,
            itemIndex == CursorItemIndex && !IsActive);
    }

    /// <summary>
    /// Runs after the current row is displayed.
    /// By default, it clears the STANDOUT and BOLD attributes.
    /// </summary>
    /// <param name="itemIndex">Which item we are currently showing</param>
    /// <param name="lineIndex">Which window line we are currently on</param>
    protected virtual void PostDisplayRow(int itemIndex, int lineIndex)
    {
        this.ToggleAttribute(CursesAttribute.STANDOUT, false);
        this.ToggleAttribute(CursesAttribute.UNDERLINE, false);
    }
}
