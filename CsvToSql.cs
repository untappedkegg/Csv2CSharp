using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace Csv2SqlCli
{
    class CsvToSql
    {
        private static readonly string[] Bool_Values = { "true", "1", "", "false", "0" };
        private static readonly string[] Bool_True_Values = Bool_Values[..1];
        private static readonly string[] Bool_False_Values = Bool_Values[3..];

        public static string SqlTableFromCsvFile(string filePath, string schema, out string tableName, char delimiter = ',')
        {

            string[] lines = File.ReadAllLines(filePath);
            string[] columnNames = lines[0].Split(delimiter).Select(str => str.Trim().Trim('"')).ToArray();
            int totalDataLines = lines.Length - 1;
            var data = lines[1..];

            tableName = Path.GetFileNameWithoutExtension(filePath).Replace('-', '_');

            string code = $"CREATE TABLE {schema ?? "dbo"}.{tableName} (\n\t";
            string columnName;
            int length = columnNames.Length;
            bool isEmpty;
            string typeString;

            for (int columnIndex = 0; columnIndex < length; columnIndex++)
            {
                columnName = Regex.Replace(Regex.Replace(columnNames[columnIndex], @"\s\(.*?\)", string.Empty), "[\\s]", "_");

                if (string.IsNullOrEmpty(columnName))
                {
                    columnName = $"Column_{columnIndex + 1}";
                }

                typeString = GetVariableDeclaration(data, columnIndex, out isEmpty, delimiter);
                code += $"{(isEmpty ? "--" : "")}{columnName} {typeString}{ ((columnIndex == length - 1) ? "" : ",")}\n\t";
            }

            code += ");\n";
            return code;
        }

        public static string GetVariableDeclaration(string[] data, int columnIndex, out bool isEmpty, char delimiter)
        {
            var rawValues = data.Select(line =>
            {

                var foo = line.Split(delimiter);
                return foo[columnIndex].Trim().Trim('"');

            });
            var hasNulls = rawValues?.Any(v => string.IsNullOrEmpty(v)) ?? false;
            string[] columnValues = rawValues.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            string typeString;
            isEmpty = false;

            if (columnValues.Length == 0)
            {
                typeString = $"{SupportedType.STRING.Name()}(250)";
                isEmpty = true;
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
            return hasNulls ? $"{typeString} NULL" : typeString;

        }

        public static bool AllBoolValues(string[] values) => values.AsParallel().All(val => bool.TryParse(val, out bool b) || Bool_Values.Contains(val.ToLower()));

        public static bool AllDoubleValues(string[] values) => values.AsParallel().All(val => double.TryParse(val, out double d));

        public static bool AllIntValues(string[] values) => values.AsParallel().All(val => int.TryParse(val, out int d));

        public static bool AllDateTimeValues(string[] values) => values.AsParallel().All(val => DateTime.TryParse(val, out DateTime d));

        // add other types if you need...
    }

}
