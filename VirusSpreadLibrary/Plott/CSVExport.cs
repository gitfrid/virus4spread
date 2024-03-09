/*
 CsvExport
 Very simple CSV-export tool for C#
 Repo: github.com/jitbit/CsvExport
 
 Usage:
	var myExport = new CsvExport();

	myExport.AddRow();
	myExport["Region"] = "Los Angeles, USA";
	myExport["Sales"] = 100000;
	myExport["Date Opened"] = new DateTime(2003, 12, 31);
		
	myExport.AddRow();
	myExport["Region"] = "Canberra \"in\" Australia";
	myExport["Sales"] = 50000;
	myExport["Date Opened"] = new DateTime(2005, 1, 1, 9, 30, 0);
	
	myExport.ExportToFile("Somefile.csv")
	
	
You can also export/compute file using any of following method:
	string myCsv = myExport.Export();
	byte[] myCsvData = myExport.ExportToBytes();
	File(myExport.ExportToBytes(), "text/csv", "results.csv");
*/

using System.Data.SqlTypes;
using System.Text;

namespace VirusSpreadLibrary.Plott
{
    public class CsvExport(string columnSeparator = ",",
                   bool includeColumnSeparatorDefinitionPreamble = true,
                   bool includeHeaderRow = true)
    {
        // To keep the list of column names with their indexes, like {"Column Name":"3"}
        private readonly Dictionary<string, int> _fields = [];

        // The list of rows
        private readonly List<List<string>> _rows = [];

        // The current row
        List<string> CurrentRow { get { return _rows[^1]; } }

        // The string used to separate columns in the output
        private readonly string _columnSeparator = columnSeparator;

        // Whether to include the preamble that declares which column separator is used in the output
        private readonly bool _includeColumnSeparatorDefinitionPreamble = includeColumnSeparatorDefinitionPreamble;

        // Whether to include the header row with column names
        private readonly bool _includeHeaderRow = includeHeaderRow;

        // Default encoding
        private readonly Encoding _defaultEncoding = Encoding.UTF8;

        // Set a value on this column
        public object this[string field]
        {
            set
            {
                // Keep track of the field names
                if (!_fields.TryGetValue(field, out int num)) //get the field's index
                {
                    //not found - add new
                    num = _fields.Count;
                    _fields.Add(field, num);
                }

                while (num >= CurrentRow.Count) //fill the current row with nulls until we have the right size
                {
                    CurrentRow.Add(null);
                }
                    

                CurrentRow[num] = MakeValueCsvFriendly(value); //set the value at position
            }
        }

        // Call this before setting any fields on a row
        public void AddRow()
        {
            _rows.Add(new(_fields.Count));
        }

        // Add a list of typed objects, maps object properties to CsvFields
        public void AddRows<T>(IEnumerable<T> list)
        {
            if (list.Any())
            {
                var values = typeof(T).GetProperties();
                foreach (T obj in list)
                {
                    AddRow();
                    foreach (var value in values)
                    {
                        this[value.Name] = value.GetValue(obj, null);
                    }
                }
            }
        }

        // Converts a value to how it should output in a csv file
        // If it has a comma, it needs surrounding with double quotes
        // Eg Sydney, Australia -> "Sydney, Australia"
        // Also if it contains any double quotes ("), then they need to be replaced with quad quotes[sic] ("")
        // Eg "Dangerous Dan" McGrew -> """Dangerous Dan"" McGrew"
        // <param name="columnSeparator">
        // The string used to separate columns in the output.
        // By default this is a comma so that the generated output is a CSV document.
        // </param>
        public static string MakeValueCsvFriendly(object value, string columnSeparator = ",")
        {
            if (value == null) return "";
            if (value is INullable nullable && nullable.IsNull) return "";

            string output;
            if (value is DateTime time)
            {
                if (time.TimeOfDay.TotalSeconds == 0)
                {
                    output = time.ToString("yyyy-MM-dd");
                }
                else
                {
                    output = time.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            else
            {
                output = value.ToString().Trim();
            }

            if (output.Length > 30000) //cropping value for stupid Excel
                output = output[..30000];

            if (output.Contains(columnSeparator) || output.Contains('"') || output.Contains('\n') || output.Contains('\r'))
                output = '"' + output.Replace("\"", "\"\"") + '"';

            return output;
        }

        // Outputs all rows as a CSV, returning one "line" at a time
        // Where "line" is a IEnumerable of string values
        private IEnumerable<IEnumerable<string>> ExportToLines()
        {
            // The header
            if (_includeHeaderRow)
                yield return _fields.OrderBy(f => f.Value).Select(f => MakeValueCsvFriendly(f.Key, _columnSeparator));

            // The rows
            foreach (var row in _rows)
            {
                yield return row;
            }
        }

        // Output all rows as a CSV returning a string
        public string Export()
        {
            StringBuilder sb = new();

            if (_includeColumnSeparatorDefinitionPreamble)
                sb.Append("sep=" + _columnSeparator + "\r\n");

            foreach (var line in ExportToLines())
            {
                foreach (var value in line)
                {
                    sb.Append(value);
                    sb.Append(_columnSeparator);
                }
                sb.Length -= _columnSeparator.Length; //remove the trailing comma (shut up)
                sb.Append("\r\n");
            }

            return sb.ToString();
        }

        // Exports to a file
        public void ExportToFile(string path, Encoding? encoding = null)
        {
            File.WriteAllBytes(path, ExportToBytes(encoding ?? _defaultEncoding));
        }

        // Exports as raw bytes.
        public byte[] ExportToBytes(Encoding? encoding = null)
        {
            using MemoryStream ms = new();
            encoding ??= _defaultEncoding;
            var preamble = encoding.GetPreamble();
            ms.Write(preamble, 0, preamble.Length);


            using (var sw = new StreamWriter(ms, encoding))
            {
                if (_includeColumnSeparatorDefinitionPreamble)
                    sw.Write("sep=" + _columnSeparator + "\r\n");

                foreach (var line in ExportToLines())
                {
                    int i = 0;
                    foreach (var value in line)
                    {
                        sw.Write(value);

                        if (++i != _fields.Count)
                            sw.Write(_columnSeparator);
                    }
                    sw.Write("\r\n");
                }

                sw.Flush(); //otherwise we're risking empty stream
            }
            return ms.ToArray();
        }
    }

}

