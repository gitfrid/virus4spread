using System.Collections.Generic;
using VirusSpreadLibrary.Creature;

namespace VirusSpreadLibrary.Grid
{
    public class CellPersons
    {
        private int numPersons;
        private List<Person> CellPersonsList { get; set; }
        public int CellNumPersons
        {
            get => numPersons;
        }
        public List<Person> GetCellPersons
        {
            get => CellPersonsList;
        }
        public CellPersons()
        {
            CellPersonsList = [];
        }
        public void CellAdd(Person AddPerson)
        {
            CellPersonsList.Add(AddPerson);
            numPersons++;

            //// just for debug
            //if (numPersons > CellPersonsList.Count)
            //    throw new CellPersonsException("CellPersons - Add Person Error");
        }
        public void CellRemove(Person RemovePerson)
        {
            CellPersonsList.Remove(RemovePerson);
            numPersons--;

            //// just for debug
            //if (numPersons < 0) 
            //    throw new CellPersonsException("CellPersons - Remove Person Error");
        }

    }
}
