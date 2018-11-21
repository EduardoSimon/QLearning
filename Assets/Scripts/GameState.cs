using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

[System.Serializable]
public class GameState
{
    public Cell.CellOwner[] Cells;
    public int IndexAction;

    public GameState(Cell.CellOwner[] cells, int indexAction)
    {
        this.Cells = cells;
        this.IndexAction = indexAction;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
			
        foreach (var cellOwner in Cells)
        {
            int i = (int)cellOwner;
            sb.AppendFormat("{0},", i);
        }

        string indexAction = IndexAction.ToString();
        return string.Format("{0}:{1}", sb, indexAction);
    }
}
	
public class GameStateComparer : IEqualityComparer<GameState>
{
    public bool Equals(GameState x, GameState y)
    {
			
        for (int i = 0; i < 9; i++)
        {
            if (x != null && !(y != null && y.Cells[i] == x.Cells[i]))
                return false;
        }

        if (y != null && (x != null && y.IndexAction != x.IndexAction))
            return false;

        return true;
    }

    public int GetHashCode(GameState obj)
    {
        int hashCode = 0;

        for (var index = 0; index < obj.Cells.Length; index++)
        {
            int owner = (int)obj.Cells[index];
            hashCode += owner * index;
        }

        hashCode += obj.IndexAction;

        return hashCode;
    }
}