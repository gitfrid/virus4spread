
namespace VirusSpreadLibrary.Grid;

public class Grid
{
    public Grid()
    {
        Cells = new GridCell[,] { };
    }
    public GridCell[,] Cells { get; set; }
    public void SetNewEmptyGrid(int MaxX, int MaxY)
    {
        Cells = new GridCell[MaxX, MaxY];

        for (int y = 0; y < MaxY; y++)
        {
            for (int x = 0; x < MaxX; x++)
            {
                this.Cells[x, y] = new GridCell();
            }
        }
    }

}
