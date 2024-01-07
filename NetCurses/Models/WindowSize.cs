namespace NetCurses.Models;
public class WindowSize
{
    public int Rows { get; set; }
    public int Columns { get; set; }
    public int RowOrigin { get; set; }
    public int ColumnsOrigin { get; set; }

    public WindowSize(int rows, int columns, int rowOrigin, int columnsOrigin)
    {
        Rows = rows;
        Columns = columns;
        RowOrigin = rowOrigin;
        ColumnsOrigin = columnsOrigin;
    }

    public int GetHorizontalCenter(int offset = 0) => (Columns / 2) + offset;
    public int GetVerticalCenter(int offset = 0) => (Rows / 2) + offset;

    public WindowSize GetHorizontalPartial(int divideBy, params int[] index)
    {
        var splitSizes = DivideHorizontally(divideBy);
        var size = splitSizes[index[0]];
        for (var i = 1; i < index.Length; i++)
        {
            size = size.MergeWith(splitSizes[index[i]]);
        }
        return size;
    }

    public WindowSize GetVerticalPartial(int divideBy, params int[] index)
    {
        var splitSizes = DivideVertically(divideBy);
        var size = splitSizes[index[0]];
        for (var i = 1; i < index.Length; i++)
        {
            size = size.MergeWith(splitSizes[index[i]]);
        }
        return size;
    }

    public WindowSize MergeWith(WindowSize target)
    {
        var rowOrigin = Math.Min(RowOrigin, target.RowOrigin);
        var columnOrigin = Math.Min(ColumnsOrigin, target.ColumnsOrigin);

        var endRow = RowOrigin + Rows;
        var targetEndRow = target.RowOrigin + target.Rows;
        var rows = Math.Max(endRow, targetEndRow) - rowOrigin;

        var endColumn = ColumnsOrigin + Columns;
        var targetEndColumn = target.ColumnsOrigin + target.Columns;
        var columns = Math.Max(endColumn, targetEndColumn) - columnOrigin;

        return new WindowSize(rows, columns, rowOrigin, columnOrigin);
    }

    public List<WindowSize> DivideHorizontally(int divideBy)
    {
        var sizes = new List<WindowSize>();
        for (var i = 0; i < divideBy; i++)
        {
            sizes.Add(new WindowSize(
                Rows,
                Columns / divideBy,
                RowOrigin,
                ColumnsOrigin + (Columns / divideBy) * i
            ));
        }
        return sizes;
    }

    public List<WindowSize> DivideVertically(int divideBy)
    {
        var sizes = new List<WindowSize>();
        for (var i = 0; i < divideBy; i++)
        {
            sizes.Add(new WindowSize(
                Rows / divideBy,
                Columns,
                RowOrigin + (Rows / divideBy) * i,
                ColumnsOrigin
            ));
        }
        return sizes;
    }

    public static WindowSize Null => new WindowSize(0, 0, 0, 0);
    public static WindowSize Fullscreen => new WindowSize(Console.WindowHeight, Console.WindowWidth, 0, 0);

    public override bool Equals(object? target)
    {
        if (target is null) return false;
        if (target is not WindowSize) return false;
        var targetSize = (WindowSize)target;
        return Rows == targetSize.Rows
            && Columns == targetSize.Columns
            && RowOrigin == targetSize.RowOrigin
            && ColumnsOrigin == targetSize.ColumnsOrigin;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Rows, Columns, RowOrigin, ColumnsOrigin);
    }
}
