﻿
namespace VirusSpreadLibrary.Creature;

public class PersonList
{
    public  List<Person> Persons { get; set; }
    public PersonList()
    {
        Persons = [];
    }
    public void SetInitialPopulation(long InitialPersonPopulation, Grid.Grid Grid)
    {
        Persons = [];

        // create initial person list at random grid coordinates
        for (int i = 0; i < InitialPersonPopulation; i++)
        {
            Person person = new() { };
            // add to list
            Persons.Add(person);
            // add person to a random home Grid Cell
            // increase population counter and set new Cell status
            person.InitBirth(Grid);
        }
    }

}
