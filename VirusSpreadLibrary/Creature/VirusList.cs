
namespace VirusSpreadLibrary.Creature;

public class VirusList
{
    public List<Virus> Viruses { get; set; }
    public VirusList()
    {
        Viruses = [];
    }
    public void SetInitialPopulation(long InitialVirusPopulation, Grid.Grid Grid)
    {
        Viruses = [];

        // create initial virus list at random grid coordinates
        for (int i = 0; i < InitialVirusPopulation; i++)
        {
            Virus virus = new();
            // add to list
            Viruses.Add(virus);
            // add Virus to a random home Grid Cell,
            // increase population counter and set new Cell status
            virus.InitReproduce(Grid);
        }
    }
   
    public void AddVirus(Virus Virus)
    {
        Viruses.Add(Virus);
    }
}