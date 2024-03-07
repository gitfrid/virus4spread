using VirusSpreadLibrary.AppProperties;
using VirusSpreadLibrary.Creature.Rates;
using VirusSpreadLibrary.SpreadModel;
using VirusSpreadLibrary.Enum;
using VirusSpreadLibrary.Grid;
using FastGraphics;


namespace VirusSpreadLibrary.Creature;

public class Person
{

    private readonly Random rnd = new();
   
    private readonly int latencyPeriod = AppSettings.Config.PersonLatencyPeriod;

    private readonly int infectiousPeriod = AppSettings.Config.PersonInfectiousPeriod;

    private readonly int reinfectionImmunityPeriod = AppSettings.Config.PersonReinfectionImmunityPeriod;

    private readonly int maxX = AppSettings.Config.GridMaxX;

    private readonly int maxY = AppSettings.Config.GridMaxY;

    private readonly PersMoveDistanceProfile persMoveProfile = new();
    private Point StartGridCoordinate { get; set; }
    private Point EndGridCoordinate { get; set; }
    private Point HomeGridCoordinate { get; set; }

    private int infectionCounter = 0;

    private int healthCounter = 0;

    private PersonState healthState = PersonState.PersonHealthy;

    private bool noTrackMovement = !AppSettings.Config.TrackMovment;

    private uint emptyCellColor = (uint)AppSettings.Config.EmptyCellColor.ToArgb();


    public int Age { get; set; }

    public Enum.CreatureType CreatureType = Enum.CreatureType.Person;
    public FastBitmap? FastBmp { get; set; }
    public int InfectionCounter
    {
        get => infectionCounter;
    }
    public PersonState HealthState
    {
        get => healthState;
    }

    private bool DoMove()
    {
        // move within PersonMoveActivityRnd percentage, 0=dont 100=always
        int moveActivity = AppSettings.Config.PersonMoveActivityRnd;
        if (moveActivity < 0) { moveActivity = 0; }
        if (moveActivity == 0 || rnd.Next(1, moveActivity + 1) > 1)
            return false;
        return true;
    }
    private bool DoMoveHome()
    {
        // move home within PersonMoveHomeActivityRnd percentage, 0=dont 100=always
        int moveActivity = AppSettings.Config.PersonMoveHomeActivityRnd;
        if (moveActivity < 0) { moveActivity = 0; }
        if (moveActivity == 0 || rnd.Next(1, moveActivity + 1) > 1) return false;
        return true;
    }
    public void CheckRecoverdImunity()
    {
        if (healthState == PersonState.PersonAfterImmunePeriode)
        {
            // person was recoverd and immune, but now can get reinfected again
            // depending on PersonReinfectionRate 
            // 50% means that 1 in 2 people who after immunity period can potentially be infected again
            if (SetPercentPossibleReinfectionsInImmunityPhase())
            {
                healthState = PersonState.PersonHealthyRecoverd;
                healthCounter = 0;
            }
        }
    }
  
    public void RunNextIteration(Grid.Grid Grid)
    {
        Age++;

        if (healthCounter != 0)
        {
            healthCounter++;
        }
        Move();
        AddToGrid(Grid);
        //DrawGrid(Grid);
    }
    public void InitBirth(Grid.Grid Grid)
    {
        // random initial move endcoordiante = homeCoordinate
        HomeGridCoordinate = new(rnd.Next(0, maxX), rnd.Next(0, maxY));
        EndGridCoordinate = HomeGridCoordinate;
        StartGridCoordinate = HomeGridCoordinate;

        // add Person to the end grid Cell and increase cellpopulation counter 
        GridCell cell = Grid.Cells[EndGridCoordinate.X, EndGridCoordinate.Y];
        cell.AddPerson(this);
    }

    private void InfectPerson()
    {
        // haelthy person got in contact with virus or contagious person
        if (healthCounter == 0)
        {
            infectionCounter++;
            healthCounter = 1;
        }
    }

    public void Move()
    {
        // get new random endpoint to move to
        // depending on the spcified range in settings of persMoveProfile and PersonMoveGlobal var
        if (DoMove())
        {
            if (AppSettings.Config.PersonMoveGlobal)
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
        EndGridCoordinate = persMoveProfile.GetEndCoordinateToMove(StartGridCoordinate);
    }
    private void MoveLocal()
    {
        // save current end as new start coordiante
        StartGridCoordinate = EndGridCoordinate;

        // calculate next move always from the Home Coordinate in the specified range - moves only within the range
        EndGridCoordinate = persMoveProfile.GetEndCoordinateToMove(HomeGridCoordinate);
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

    public void SpreadVirus(Grid.Grid Grid, VirusList VirusList)
    {
        // infectous people breed virus
        if (healthState == PersonState.PersonInfectious)
        {
            // Probability of reproduction 100% = one virus every day/iteration is reproduced during infectious phase of a person
            // 0% = no virus reproduction 
            double randomProbability = AppSettings.Config.VirusReproductionRate;
            if (randomProbability >= 0 && randomProbability <= 100)
            {
                double randomNumber = rnd.NextDouble() * 100;
                if (randomNumber <= randomProbability)
                {
                    // VirusNumberReproducedPerIteration means number of viruses reproduced per iteration
                    for (int i = 0; i < AppSettings.Config.VirusNumberReproducedPerIteration; i++)
                    {
                        Virus virusNew = new();
                        virusNew.ReproduceToGrid(VirusList, EndGridCoordinate, Grid);
                    }
                };
            }
            else
            {
                // wrong input from AppSettings - should not happen
                throw new VirusReproductionRateInputException("");
            }
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
        
        if (noTrackMovement) 
        {            
            uint resultStartVir = emptyCellColor;
            uint alphaStartVir = 0x00000000;
            pixels[StartGridCoordinate.Y * maxX + StartGridCoordinate.X] = alphaStartVir | resultStartVir;
        }

        GridCell cell = Grid.Cells[EndGridCoordinate.X, EndGridCoordinate.Y];
        uint result = (uint)cell.CellColor.ToArgb();
        //uint result = (uint)Color.Green.ToArgb();
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

        // remove Person from start grid Cell and decrease cellpopulation counter 
        GridCell cellStart = Grid.Cells[StartGridCoordinate.X, StartGridCoordinate.Y];
        cellStart.RemovePerson(this);

        // add Person to the end grid Cell and increase cellpopulation counter 
        GridCell cell = Grid.Cells[EndGridCoordinate.X, EndGridCoordinate.Y];
        cell.AddPerson(this);
    }

    public void SetCellSateAndInfect(Grid.Grid Grid)
    {
        // if a virus or contagious person arrived on a clean cell after movement
        // then infect all Persons on that grid cell
                
        GridCell cell = Grid.Cells[EndGridCoordinate.X, EndGridCoordinate.Y];

        if (cell.IsInfectiousAfterMovement == true) 
        {
            foreach (Person pers in cell.PersonPopulation.GetCellPersons)
            {
                pers.InfectPerson();
                pers.SetPersonState();
            }
        }
        SetPersonState();
        cell.SetNewCellState();
    }

    private void SetPersonState()
    {
        if (healthCounter == 0)
        {
            // person healthy or recoverd
            if (infectionCounter < 1)
            {
                healthState = PersonState.PersonHealthy;
            }
            else
            {
                healthState = PersonState.PersonHealthyRecoverd;
            }
            return;
        }

        if (healthCounter <= latencyPeriod)
        {
            // LatencyPeriod - person was infected
            if (infectionCounter < 2)
            {
                healthState = PersonState.PersonInfected;
            }
            else
            {
                healthState = PersonState.PersonReinfected;
            }
        }

        if (healthCounter > latencyPeriod
            && healthCounter <= infectiousPeriod + latencyPeriod)
        {
            // InfectiousPeriod - person infectious
            healthState = PersonState.PersonInfectious;
        }

        if (healthCounter > infectiousPeriod + latencyPeriod
            && healthCounter <= reinfectionImmunityPeriod + infectiousPeriod + latencyPeriod)
        {

            // ReinfectionImmunityPeriod - person was recoverd and is imune - immunity periode
            healthState = PersonState.PersonRecoverdImmunePeriodNotInfectious;
        }

        if (healthCounter > reinfectionImmunityPeriod + infectiousPeriod + latencyPeriod)
        {
            // PersonAfterImmunePeriode - person can get reinfected again depending on the PersonReinfectionRate
            healthState = PersonState.PersonAfterImmunePeriode;
        }
    }

    // 50% means that 1 in 2 people who after immunity period can potentially be infected again
    // 1 day = 1 iteration over the entire population
    // Is this model realistic, does it depend only on the person and the virus, or also on external factors?
    // Depending on how the reinfection model is implemented, it makes a big difference to the outcome
    private bool SetPercentPossibleReinfectionsInImmunityPhase()
    {
        if (AppSettings.Config.PersonReinfectionRate == 0)
        {
            return false;
        }
        if (AppSettings.Config.PersonReinfectionRate == 100)
        {
            return true;
        }

        double randomProbability = AppSettings.Config.PersonReinfectionRate;
        if (randomProbability > 0 && randomProbability < 100)
        {
            double randomNumber = rnd.NextDouble() * 100;
            if (randomNumber <= randomProbability)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            // wrong input from AppSettings - should not happen
            throw new PersonReinfectionRateInputException("");
        }
    }
} 
