using FastGraphics;
using VirusSpreadLibrary.AppProperties;
using VirusSpreadLibrary.Creature.Rates;
using VirusSpreadLibrary.Grid;

namespace VirusSpreadLibrary.Creature;

public class Virus
{
    private readonly Random rnd = new ();

    public Enum.CreatureType CreatureType = Enum.CreatureType.Virus;
    public int Age { get; set; }

    private readonly int maxX = AppSettings.Config.GridMaxX;

    private readonly int maxY = AppSettings.Config.GridMaxY;

    public FastBitmap? FastBmp { get; set; }

    // move data
    private readonly VirMoveDistanceProfile virMoveProfile = new();

    private uint emptyCellColor = (uint)AppSettings.Config.EmptyCellColor.ToArgb();


    public Point StartGridCoordinate { get; private set; }
    public Point EndGridCoordinate { get; private set; }
    public Point HomeGridCoordinate { get; private set; }
    
    public bool DoMove()
    {
        int moveActivity = AppSettings.Config.VirusMoveActivityRnd;
        if (moveActivity == 0 || rnd.Next(1, moveActivity + 1) > 1) return false;
        return true;
    }
    public bool DoMoveHome()
    {
        int moveActivity = AppSettings.Config.VirusMoveHomeActivityRnd;
        if (moveActivity == 0 || rnd.Next(1, moveActivity + 1) > 1) return false;
        return true;
    }

    public void RunNextIteration(Grid.Grid Grid)
    {
        Age++;
        Move();
        AddToGrid(Grid);        
    }
    public void VirusRemove(VirusList VirList, int x)
    {
        // remove decayed virus if VirusInfectionDurationDays were reached
        if ((Age > AppSettings.Config.VirusInfectionDurationDays) && (AppSettings.Config.VirusInfectionDurationDays != 0))
        {
            // delete from grid if virus is defect
            if (FastBmp == null)
            {
                return;
            }
            uint[] pixels = FastBmp.Pixels;

            uint resultStartVir = (uint)Color.Black.ToArgb();
            uint alphaStartVir = 0xFF000000;
            pixels[EndGridCoordinate.Y * maxX + EndGridCoordinate.X] = alphaStartVir | resultStartVir;
            
            VirList.Viruses.RemoveAt(x);
        }
    }
    public void InitReproduce(Grid.Grid Grid)
    {
        // random initial move endcoordiante = homeCoordinate
        HomeGridCoordinate = new(rnd.Next(0, maxX), rnd.Next(0, maxY));
        EndGridCoordinate = HomeGridCoordinate;
        StartGridCoordinate = HomeGridCoordinate;
        
        // add Virus to the end grid Cell and increase cellpopulation counter 
        GridCell cell = Grid.Cells[EndGridCoordinate.X, EndGridCoordinate.Y];
        cell.AddVirus(this);
    }

    public void ReproduceToGrid(VirusList VirusList, Point ReproduceCoordiante, Grid.Grid Grid)
    {
        // random initial move endcoordiante = homeCoordinate
        EndGridCoordinate = ReproduceCoordiante;
        HomeGridCoordinate = ReproduceCoordiante;
        StartGridCoordinate = ReproduceCoordiante;

        // add Virus to the end grid Cell and increase cellpopulation counter 
        GridCell cell = Grid.Cells[EndGridCoordinate.X, EndGridCoordinate.Y];
        VirusList.AddVirus(this);
        cell.AddVirus(this);        
        SetCellSate(Grid);
    }

    public void Move()
    {
        // get new random endpoint to move to
        // depending on the spcified range in settings of virMoveProfile and VirusMoveGlobal var
        if (DoMove())
        {
            if (AppSettings.Config.VirusMoveGlobal)
            {
                if (DoMoveHome())
                {
                    MoveHome();
                }
                else
                {
                    MoveGlobal();
                }
            }
            else
            {
                if (DoMoveHome())
                {
                    MoveHome();
                }
                else
                {
                    MoveLocal();
                }
            }
        }
    }

    private void MoveGlobal()
    {
        // save current end as new start coordiante
        StartGridCoordinate = EndGridCoordinate;

        // calculate next move from EndCoordinate of the last iteration, in the spcified range - moves over whole grid
        EndGridCoordinate = virMoveProfile.GetEndCoordinateToMove(StartGridCoordinate);
    }
    private void MoveLocal()
    {
        // save current end as new start coordiante
        StartGridCoordinate = EndGridCoordinate;

        // calculate next move always from the Home Coordinate in the specified range - moves only within the range
        EndGridCoordinate = virMoveProfile.GetEndCoordinateToMove(HomeGridCoordinate);
    }

    private void MoveHome()
    {
        // save current end as new start coordiante
        StartGridCoordinate = EndGridCoordinate;

        // move back to Home Coordinate 
        EndGridCoordinate = HomeGridCoordinate;
    }
    public double GetMoveDistance()
    {
        System.Drawing.Point startPoint = StartGridCoordinate;
        System.Drawing.Point endPoint = EndGridCoordinate;
        // calc move distance
        if (startPoint == endPoint)
        {
            return 0;
        }
        else
        {
            int dx = endPoint.X - startPoint.X;
            int dy = endPoint.Y - startPoint.Y;
            double SE = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
            return SE;
        }
    }
    public void DrawGrid(Grid.Grid Grid)
    {
        // draw to the grid after move
        if (FastBmp == null)
        {
            return;
        }
        uint[] pixels = FastBmp.Pixels;

        if (!AppSettings.Config.TrackMovment)
        {
            uint resultStartVir = emptyCellColor;
            uint alphaStartVir = 0x00000000;
            pixels[StartGridCoordinate.Y * maxX + StartGridCoordinate.X] = alphaStartVir | resultStartVir;
        }

        GridCell cell = Grid.Cells[EndGridCoordinate.X, EndGridCoordinate.Y];
        uint result = (uint)cell.CellColor.ToArgb();
        //uint result = (uint)Color.Red.ToArgb();
        uint alpha = 0xFF000000;
        pixels[EndGridCoordinate.Y * maxX + EndGridCoordinate.X] = alpha | result;

    }

    private void AddToGrid(Grid.Grid Grid)
    {

        // if virus not moved do nothing
        if (StartGridCoordinate.X == EndGridCoordinate.X & StartGridCoordinate.Y == EndGridCoordinate.Y)
        {
            return;
        }

        // remove Virus from start grid Cell and decrease cellpopulation counter         
        GridCell cellStart = Grid.Cells[StartGridCoordinate.X, StartGridCoordinate.Y];
        cellStart.RemoveVirus(this);

        // add Virus to the end grid Cell and increase cellpopulation counter 
        GridCell cell = Grid.Cells[EndGridCoordinate.X, EndGridCoordinate.Y];
        cell.AddVirus(this);
    }
    public void SetCellSate(Grid.Grid Grid)
    {
        // if a virus or contagious person arrived on a clean cell after movement
        // then infect all Persons on that grid cell
        GridCell cell = Grid.Cells[EndGridCoordinate.X, EndGridCoordinate.Y];
        cell.SetNewCellState();
    }
}
