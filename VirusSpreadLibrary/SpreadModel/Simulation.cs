
using VirusSpreadLibrary.Creature;
using VirusSpreadLibrary.AppProperties;
using VirusSpreadLibrary.Plott;
using VirusSpreadLibrary.Grid;



namespace VirusSpreadLibrary.SpreadModel;

public class Simulation
{

    private readonly Grid.Grid grid;

    private readonly PersonList personList = new();
    private readonly VirusList virusList = new();
    public FastBitmap? FastBitmap;

    private int iteration;    
    private bool stopIteration;
    readonly private PlotData plotData = new();

    // public prop to access the queue
    public PlotData PlotData { get => plotData; }
    public int MaxX { get; set; }
    public int MaxY { get; set; }
    public int Iteration { get => iteration; }
    public bool IterationRunning { get => !stopIteration; }

    public bool IsMinimizedGridForm = false;

    public Simulation()
    {
        stopIteration = true;
        MaxX = AppSettings.Config.GridMaxX;
        MaxY = AppSettings.Config.GridMaxY;
        grid = new Grid.Grid();
        grid.SetNewEmptyGrid(MaxX, MaxY);
        personList.SetInitialPopulation(AppSettings.Config.InitialPersonPopulation, grid);
        virusList.SetInitialPopulation(AppSettings.Config.InitialVirusPopulation, grid);
        CreateFastBitmap(MaxX, MaxY);
    }
    
    
    // dispose of existing fast bitmap (if exists)
    // create new fast bitmap based upon client rectangle.
    public void CreateFastBitmap(int WidthX, int HeigthY)
    {
        FastBitmap?.Dispose();
        FastBitmap = new FastBitmap(WidthX, HeigthY);
    }

 
    // Public implementation of Dispose pattern callable by consumers.
    public void Dispose()
    {
        Dispose(true);
    }

    // Protected implementation of Dispose pattern.
    protected virtual void Dispose(bool disposing)
    {
            if (disposing)
            {
                FastBitmap?.Dispose();
            }
    }
    public void StartIteration()
    {
        stopIteration = false;
    }
    public void StopIteration()
    {
        stopIteration = true;
    }
    public void NextIteration()
    {
        if (stopIteration == true) { return; }
        
        //Log.Logger = Logging.GetInstance();        
        //Log.Logger.Information("Nr: {A} iteration", iteration);
        iteration++;
        plotData.IterationNumber = iteration;

        long personAgeCum = 0;
        double personsMoveDistanceCum = 0;
        plotData.PersonPopulation = personList.Persons.Count;

        if (FastBitmap != null)
        {
            foreach (Person person in personList.Persons)
            {
                person.FastBmp = FastBitmap;
                person.RunNextIteration(grid);
                personAgeCum += person.Age;
                personsMoveDistanceCum += person.GetMoveDistance();
            }
        }

        long virusAgeCum = 0;
        double virusesMoveDistanceCum = 0;
        plotData.VirusPopulation = virusList.Viruses.Count;
        if (FastBitmap != null)
        {
            for (int x = virusList.Viruses.Count - 1; x > -1; x--)
            {
                Virus virus = virusList.Viruses[x];
                virus.FastBmp = FastBitmap;
                virus.RunNextIteration(grid);
                virusAgeCum += virus.Age;
                virusesMoveDistanceCum += virus.GetMoveDistance();
                // virus end of life reached
                virus.VirusRemove(virusList,x);
            };

        }

        foreach (Virus virus in virusList.Viruses)
        {
            virus.SetCellSate(grid);
            virus.DrawGrid(grid);
        }

        foreach (Person person in personList.Persons)
        {        
            // infect person depending if cell contains virus or infectious person
            person.SetCellSateAndInfect(grid);
            
            // spread new viruses at current coordinate if person is in infectious periode
            // depending on VirusReproductionRate and VirusNumberReproducedPerIteration
            person.SpreadVirus(grid, virusList);

            // person was recoverd and immune, but now can get reinfected again
            // depending on PersonReinfectionRate 
            person.CheckRecoverdImunity();

            person.DrawGrid(grid);

            // get the plot data
            
            plotData.SetPlotHealthState(person);
        }

        if (plotData.PersonPopulation > 0)
        {
            plotData.PersonsAge = personAgeCum / plotData.PersonPopulation;
            plotData.PersonsMoveDistance = personsMoveDistanceCum / plotData.PersonPopulation;
            plotData.PersonsInfectionCounter /= plotData.PersonPopulation;
        }

        if (plotData.VirusPopulation > 0)
        {
            plotData.VirusesAge = virusAgeCum / plotData.VirusPopulation; //<- cumulated age
            plotData.VirusesMoveDistance = virusesMoveDistanceCum / plotData.VirusPopulation; //<- cumulated move distance
        }

        // debug
        //plotData.PersonPopulation = plotData.PersonsInfected + plotData.PersonsHealthy + plotData.PersonsInfectious + plotData.PersonsRecoverd + PlotData.PersonsRecoverdImmuneNotinfectious + plotData.PersonsReinfected + plotData.PersonAfterImmunePeriode;

        // fix if person reinfecton rate <> 100, because a percentage of persons reach the status PersonAfterImmunePeriode 
        // which i am counting (adding) to PersonsRecoverdImmuneNotinfectious but not plotting or showing as separate status
        plotData.PersonsRecoverdImmuneNotinfectious += plotData.PersonAfterImmunePeriode;
        // write data to queue for plotting and reset queue
        plotData.WriteToQueue();
        plotData.ResetCounter();

    }
 }