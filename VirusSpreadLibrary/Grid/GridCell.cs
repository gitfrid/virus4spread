using System.Drawing;

using VirusSpreadLibrary.Creature;
using VirusSpreadLibrary.Enum;

namespace VirusSpreadLibrary.Grid;
public class GridCell
{
    private Color cellColor;   
    private readonly CellViruses virusPopulation;
    private readonly CellPersons personPopulation;
    private CellState cellState = Enum.CellState.EmptyCell;
    private bool isInfectiousBeforeMovement = false;
    private bool isInfectiousAfterMovement = false;

    private int numViruses = 0;
    private int numPersons = 0;
    public GridCell()
    {
        // set cell population and color!
        virusPopulation = new CellViruses();
        personPopulation = new CellPersons();
        cellColor = new Color();         
    }
    private CellViruses VirusPopulation
    {
        get => virusPopulation;
    }
    private CellState CellState
    {
        get => cellState;
        set => cellState = value;
    }
    private bool CheckIfInfectious()
    {
        if (numViruses > 1)
        {
            return true;
        }
        if (numPersons > 1)
        {
            foreach (Person Person in personPopulation.GetCellPersons)
            {
                if (Person.HealthState == PersonState.PersonInfectious)
                {
                    return true;
                }
            }
        }
        return false;
    }
    public Color CellColor
    {
        get => cellColor!;
        set => cellColor = value;
    }

    public bool IsInfectiousAfterMovement
    {
        get => isInfectiousBeforeMovement;
    }
    public CellPersons PersonPopulation
    {
        get => personPopulation;
    }
    public void AddVirus(Virus AddVirus )
    {
        VirusPopulation.CellAdd(AddVirus);
        numViruses++;
        isInfectiousAfterMovement = true;
    }
    public void AddPerson(Person AddPerson)
    {
        PersonPopulation.CellAdd(AddPerson);
        numPersons++;
        if (AddPerson.HealthState == PersonState.PersonInfectious)
        {
            isInfectiousAfterMovement = true;
        }
    }            
    public void RemoveVirus(Virus RemoveVirus)
    {
        VirusPopulation.CellRemove(RemoveVirus);
        numViruses--;
        isInfectiousAfterMovement = CheckIfInfectious();
    }
    public void RemovePerson(Person RemovePerson)
    {
        personPopulation.CellRemove(RemovePerson);
        numPersons--;
        isInfectiousAfterMovement = CheckIfInfectious();
    }
   
    public void SetNewCellState()
    {
        // set new isInfectiousBeforeMovement value for next iteration
        isInfectiousBeforeMovement = isInfectiousAfterMovement;

        // evaluate new state of grid cell
        if (numPersons > 0)
        {
            bool personsInfectious = false;
            bool personsInfected = false;
            bool personRecoverdImmuneNotInfectious = false;

            // get current state of the grid Cell  
            for (int i = 0; i < numPersons; i++)
            {
                if (PersonState.PersonReinfected == PersonPopulation.GetCellPersons[i].HealthState || PersonState.PersonInfected == PersonPopulation.GetCellPersons[i].HealthState)
                {
                    personsInfected = true;
                }

                if (PersonState.PersonInfectious == PersonPopulation.GetCellPersons[i].HealthState)
                {
                    personsInfectious = true;
                }

                if (PersonState.PersonRecoverdImmunePeriodNotInfectious == PersonPopulation.GetCellPersons[i].HealthState)
                {
                    personRecoverdImmuneNotInfectious = true;
                }
            }

            // set new state of the grid Cel -> represent as Grid color in following ranking order
            CellState = CellState.PersonsHealthyOrRecoverd;

            if (personRecoverdImmuneNotInfectious)
            {
                CellState = CellState.PersonsRecoverdImmuneNotInfectious;
            }

            if (personsInfected) 
            {
                CellState = CellState.PersonsInfected;
            }

            if (personsInfectious)
            {
                CellState = CellState.PersonsInfectious;
            }
        }

        // if no persons on a cell, two possible Grid colors exist
        if (numViruses < 1 && numPersons < 1)
        {
            CellState = CellState.EmptyCell;
        }

        if (numViruses > 0 && numPersons < 1)
        {
            CellState = CellState.Virus;
        }

        // set color depending on cell sate
        cellColor = ColorList.GetCellColor(CellState,numPersons,numViruses);
    }
}
