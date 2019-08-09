using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace Csv2CSharpCli
{
    class CsvToClass
    {
        private static readonly string[] Bool_Values = { "true", "y", "yes", "on", "false", "n", "no", "off" };
        private static readonly string[] Bool_True_Values = Bool_Values[..3];
        private static readonly string[] Bool_False_Values = Bool_Values[4..];

        public static string CSharpClassCodeFromCsvFile(string filePath, out string className, char delimiter = ',',
            string classAttribute = "", string propertyAttribute = "")
        {
            if (!string.IsNullOrWhiteSpace(propertyAttribute))
            {
                propertyAttribute += "\n\t\t";
            }

            if (!string.IsNullOrWhiteSpace(classAttribute))
            {
                classAttribute += "\n";
            }

            string[] lines = File.ReadAllLines(filePath);
            string[] columnNames = lines[0].Split(',').Select(str => str.Trim()).ToArray();
            int totalDataLines = lines.Length - 1;
            var data = lines[1..];

            className = Path.GetFileNameWithoutExtension(filePath).Replace('-', '_');

            string code = $"using System;\nusing CsvHelper.Configuration.Attributes;\n\nnamespace Csv2CSharp \n{{\n{classAttribute}\tpublic class {className} \n\t{{ \n";

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
            SupportedType actualType;

            if (columnValues.Length == 0)
            {
                actualType = SupportedType.STRING;
            }
            else if (AllIntValues(columnValues))
            {
                actualType = SupportedType.INT;
            }
            else if (AllDoubleValues(columnValues))
            {
                actualType = SupportedType.DOUBLE;
            }
            else if (AllDateTimeValues(columnValues))
            {
                actualType = SupportedType.DATETIME;
            }
            else if (AllBoolValues(columnValues))
            {
                actualType = SupportedType.BOOL;
            }
            else
            {
                actualType = SupportedType.STRING;
            }

            return $"{attribute}{GetAdditionalAttributeForType(actualType, columnValues)}public {actualType.Name()} {columnName} {{ get; set; }}";
        }

        private static string GetAdditionalAttributeForType(SupportedType type, string[] columnValues)
        {
            switch (type)
            {
                case SupportedType.INT:
                    break;
                case SupportedType.DOUBLE:
                    break;
                case SupportedType.DATETIME:
                    break;
                case SupportedType.BOOL:
                    var vals = columnValues.Distinct();
                    var trueVals = vals.Intersect(Bool_True_Values, StringComparer.OrdinalIgnoreCase).Select(s => $"\"{s}\"");
                    var falseVals = vals.Intersect(Bool_False_Values, StringComparer.OrdinalIgnoreCase).Select(s => $"\"{s}\"");

                    return $"[BooleanTrueValues({string.Join(',', trueVals)})]\n\t\t[BooleanFalseValues({string.Join(',', falseVals)})]\n\t\t";         
                case SupportedType.STRING:
                default:
                    return string.Empty;
            }
            return string.Empty;
        }

        public static bool AllBoolValues(string[] values)
        {
            return values.All(val => bool.TryParse(val, out bool b) || Bool_Values.Contains(val.ToLower()));
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
