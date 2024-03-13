using System.Collections.Generic;
using VirusSpreadLibrary.Creature;

namespace VirusSpreadLibrary.Grid
{
    public class CellViruses
    {
        private int numViruses;
        private List<Virus> CellVirusesList { get; set; }

        public int CellNumViruses
        {
            get => numViruses;

        }
        public List<Virus> GetCellViruses
        {
            get => CellVirusesList;
        }
        public CellViruses()
        {
            CellVirusesList = [];
        }
        public void CellAdd(Virus AddVirus)
        {
            CellVirusesList.Add(AddVirus);
            ++numViruses;

            //// just for debug
            //if (numViruses > Viruses.Count)
            //    throw new CellVirusesException("CellViruses - Add Virus Error");
        }
        public void CellRemove(Virus RemoveVirus)
        {
            //// just for debug
            //if (Viruses.Count != numViruses)
            //    throw new CellVirusesException("CellViruses - Remove Virus Error in numViruses count");

            CellVirusesList.Remove(RemoveVirus);
            --numViruses;
        }

    }
}
