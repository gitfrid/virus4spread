using VirusSpreadLibrary.Enum;
using VirusSpreadLibrary.AppProperties;


namespace VirusSpreadLibrary.Grid;

public static class CellStateExtions
{
    // create a static dictionary to store the colors
    private static readonly Dictionary<CellState, Color> colorMap;
   
    // initialize the dictionary in a static constructor
    static CellStateExtions()
    {
        colorMap = new Dictionary<CellState, Color>
        {
            { CellState.PersonsHealthyOrRecoverd, AppSettings.Config.PersonsHealthyOrRecoverdColor },
            { CellState.PersonsInfected, AppSettings.Config.PersonInfectedColor },
            { CellState.PersonsInfectious, AppSettings.Config.PersonInfectiousColor },
            { CellState.PersonsRecoverdImmuneNotInfectious, AppSettings.Config.PersonsRecoverdImmuneNotInfectiousColor },
            { CellState.Virus, AppSettings.Config.VirusColor },
            { CellState.EmptyCell, AppSettings.Config.EmptyCellColor }
        };
    }
    public static Color ToTheColor(this CellState cellState)
    {
        // try to get the color from the dictionary
        if (colorMap.TryGetValue(cellState, out Color color))
        {
            return color;
        }
        else
        {
            // throw an exception if the cell state is not valid
            throw new ArgumentOutOfRangeException(nameof(cellState), cellState, null);
        }
    }
}

public class ColorList
{

    public static Color GetCellColor(CellState cellState, int personPopulation, int virusPopulation)
    {
        var cellColor = cellState.ToTheColor();

        if (AppSettings.Config.PopulationDensityColoring && cellState != CellState.EmptyCell)
        {
            var population = Math.Max(personPopulation, virusPopulation);
            if (population > 1)
            {
                cellColor = DarkenColor(cellColor, 1 / (float)population);
            }
        }
        return cellColor;
    }
    public static Color DarkenColor(Color color, float percent)
    {
        return ChangeColorBrightness(color, -1 * percent);
    }
    //private static Color LightenColor(Color color, float percent)
    //{
    //    return ChangeColorBrightness(color, percent);
    //}
    private static Color ChangeColorBrightness(Color color, float correctionFactor)
    {
        float red = (float)color.R;
        float green = (float)color.G;
        float blue = (float)color.B;

        if (correctionFactor < 0)
        {
            correctionFactor = 1 + correctionFactor;
            red *= correctionFactor;
            green *= correctionFactor;
            blue *= correctionFactor;
        }
        else
        {
            red = (255 - red) * correctionFactor + red;
            green = (255 - green) * correctionFactor + green;
            blue = (255 - blue) * correctionFactor + blue;
        }

        return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
    }  

}

