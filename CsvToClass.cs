using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace Csv2CSharpCli
{
    class CsvToClass
    {
        public static string CSharpClassCodeFromCsvFile(string filePath, out string className, char delimiter = ',',
            string classAttribute = "", string propertyAttribute = "")
        {
            if (!string.IsNullOrWhiteSpace(propertyAttribute))
            {
                propertyAttribute += "\n\t\t";
            }

            if (!string.IsNullOrWhiteSpace(propertyAttribute))
            {
                classAttribute += "\n";
            }

            string[] lines = File.ReadAllLines(filePath);
            string[] columnNames = lines[0].Split(',').Select(str => str.Trim()).ToArray();
            int totalDataLines = lines.Length - 1;
            var data = lines[1..];

            className = Path.GetFileNameWithoutExtension(filePath).Replace('-', '_');

            string code = $"using System;\n\nnamespace Csv2CSharp \n{{\n{classAttribute}\tpublic class {className} \n\t{{ \n";

            for (int columnIndex = 0; columnIndex < columnNames.Length; columnIndex++)
            {
                var columnName = Regex.Replace(columnNames[columnIndex], @"[\s\.\""\(.*?\)-]", string.Empty);
                if (string.IsNullOrEmpty(columnName))
                    columnName = "Column" + (columnIndex + 1);
                code += $"\n\t\t[Name({columnNames[columnIndex]})]";
                code += $"\n\t\t{GetVariableDeclaration(data, columnIndex, columnName, propertyAttribute, delimiter)}\n";
            }

            code += "\t}\n}\n";
            return code;
        }

        public static string GetVariableDeclaration(string[] data, int columnIndex, string columnName, string attribute = null, char delimiter = ',')
        {
            string[] columnValues = data.Select(line => line.Split(delimiter)[columnIndex].Trim().Trim('"')).Where(s => !string.IsNullOrEmpty(s)).ToArray();
            string typeAsString;

            if (columnValues.Length == 0)
            {
                typeAsString = "string";
            }
            else if (AllIntValues(columnValues))
            {
                typeAsString = "int";
            }
            else if (AllDoubleValues(columnValues))
            {
                typeAsString = "double";
            }
            else if (AllDateTimeValues(columnValues))
            {
                typeAsString = "DateTime";
            }
            else
            {
                typeAsString = "string";
            }

            return $"{attribute}public {typeAsString} {columnName} {{ get; set; }}";
        }

        public static bool AllDoubleValues(string[] values)
        {
            return values.All(val => double.TryParse(val, out double d));
        }

        public static bool AllIntValues(string[] values)
        {
            return values.All(val => int.TryParse(val, out int d));
        }

        public static bool AllDateTimeValues(string[] values)
        {
            return values.All(val => DateTime.TryParse(val, out DateTime d));
        }

        // add other types if you need...
    }
}
