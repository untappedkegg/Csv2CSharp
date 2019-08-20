using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace Csv2SqlCli
{
    class CsvToSql
    {
        private static readonly string[] Bool_Values = { "true", "1", "false", "0" };
        private static readonly string[] Bool_True_Values = Bool_Values[..1];
        private static readonly string[] Bool_False_Values = Bool_Values[2..];

        public static string CSharpClassCodeFromCsvFile(string filePath, string schema, out string tableName, char delimiter = ',')
        {

            string[] lines = File.ReadAllLines(filePath);
            string[] columnNames = lines[0].Split(',').Select(str => str.Trim()).ToArray();
            int totalDataLines = lines.Length - 1;
            var data = lines[1..];

            tableName = Path.GetFileNameWithoutExtension(filePath).Replace('-', '_');

            string code = $"CREATE TABLE {schema ?? "dbo"}.{tableName} (\n\t";
            string columnName;
            int length = columnNames.Length;


            for (int columnIndex = 0; columnIndex < length; columnIndex++)
            {
                columnName = Regex.Replace(Regex.Replace(columnNames[columnIndex], @"\s\(.*?\)", string.Empty), "[\\s]", "_");

                if (string.IsNullOrEmpty(columnName))
                {
                    columnName = "Column" + (columnIndex + 1);
                }
                code += $"{columnName.Trim('"')} {GetVariableDeclaration(data, columnIndex, columnName, delimiter)} NULL{ ((columnIndex == length - 1) ? "" : ",")}\n\t";

            }

            code += ")\n";
            return code;
        }

        public static string GetVariableDeclaration(string[] data, int columnIndex, string columnName, char delimiter = ',')
        {
            string[] columnValues = data.Select(line => line.Split(delimiter)[columnIndex].Trim().Trim('"')).Where(s => !string.IsNullOrEmpty(s)).ToArray();
            string typeString;

            if (columnValues.Length == 0)
            {
                typeString = $"{SupportedType.STRING.Name()}(250)";
            }
            else if (AllIntValues(columnValues))
            {
                typeString = SupportedType.INT.Name();
            }
            else if (AllDoubleValues(columnValues))
            {
                typeString = SupportedType.DOUBLE.Name();
            }
            else if (AllDateTimeValues(columnValues))
            {
                typeString = SupportedType.DATETIME.Name();
            }
            else if (AllBoolValues(columnValues))
            {
                typeString = SupportedType.BOOL.Name();

            }
            else
            {
                int maxLength = columnValues.AsParallel().Max(a => a.Length);
                typeString = $"{SupportedType.STRING.Name()}({maxLength})";
            }
            return typeString;
            
        }

        public static bool AllBoolValues(string[] values)
        {
            return values.AsParallel().All(val => bool.TryParse(val, out bool b) || Bool_Values.Contains(val.ToLower()));
        }

        public static bool AllDoubleValues(string[] values)
        {
            return values.AsParallel().All(val => double.TryParse(val, out double d));
        }

        public static bool AllIntValues(string[] values)
        {
            return values.AsParallel().All(val => int.TryParse(val, out int d));
        }

        public static bool AllDateTimeValues(string[] values)
        {
            return values.AsParallel().All(val => DateTime.TryParse(val, out DateTime d));
        }

        // add other types if you need...
    }

}
