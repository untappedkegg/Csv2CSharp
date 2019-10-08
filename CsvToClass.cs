using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Csv2CSharpCli
{
    class CsvToClass
    {
        private static readonly string[] Bool_Values = { "true", "y", "yes", "on", "false", "n", "no", "off" };
        private static readonly string[] Bool_True_Values = Bool_Values[..3];
        private static readonly string[] Bool_False_Values = Bool_Values[4..];

        public static string CSharpClassCodeFromCsvFile(string filePath, out string className, char delimiter = ',', string classAttribute = "", string propertyAttribute = "")
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
            string[] columnNames = lines[0].Split(delimiter).Select(str => str.Trim()).ToArray();
            int totalDataLines = lines.Length - 1;
            var data = lines[1..];

            className = Path.GetFileNameWithoutExtension(filePath).Replace('-', '_');

            //Get the cultureInfo in order to handle casing
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            string code = $"using System;\nusing CsvHelper.Configuration.Attributes;\nusing System.ComponentModel.DataAnnotations.Schema;\n\nnamespace Csv2CSharp \n{{\n{classAttribute}\tpublic class {textInfo.ToTitleCase(className).Replace(" ", "")} \n\t{{ \n";
            string colNameNoParens, columnName;


            for (int columnIndex = 0; columnIndex < columnNames.Length; columnIndex++)
            {
                colNameNoParens = Regex.Replace(columnNames[columnIndex], @"\s\(.*?\)", string.Empty);
                columnName = Regex.Replace(colNameNoParens, @"[\s\.\""-]", string.Empty);
                if (string.IsNullOrEmpty(columnName)) {
                    columnName = "Column" + (columnIndex + 1);
                }
                var declaration = GetVariableDeclaration(data, columnIndex, columnName, propertyAttribute, delimiter, out bool isEmpty);

                if (isEmpty)
                {
                    code += $"\n\t\t//[Name(\"{columnNames[columnIndex]}\")]\n\t\t//[Column(\"{Regex.Replace(colNameNoParens, "[\\s]", "_")}\")]";
                } else
                {
                code += $"\n\t\t[Name(\"{columnNames[columnIndex]}\")]\n\t\t[Column(\"{Regex.Replace(colNameNoParens, "[\\s]", "_")}\")]";
                }
                code += $"\n\t\t{declaration}\n";
            }

            code += "\t}\n}\n";
            return code;
        }

        public static string GetVariableDeclaration(string[] data, int columnIndex, string columnName, string attribute, char delimiter, out bool isEmpty)
        {
            var rawValues = data.Select(line => line.Split(delimiter)[columnIndex].Trim().Trim('"'));
            var hasNulls = rawValues?.Any(v => string.IsNullOrEmpty(v)) ?? false;
            string[] columnValues = rawValues.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            SupportedType actualType;
            string additionalAttributes = string.Empty;
            isEmpty = false;

            if (columnValues.Length == 0)
            {
                actualType = SupportedType.STRING;
                isEmpty = true;
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
                var vals = columnValues.Distinct();
                var trueVals = vals.Intersect(Bool_True_Values, StringComparer.OrdinalIgnoreCase).Select(s => $"\"{s}\"");
                var falseVals = vals.Intersect(Bool_False_Values, StringComparer.OrdinalIgnoreCase).Select(s => $"\"{s}\"");


                additionalAttributes = $"[BooleanTrueValues({string.Join(',', trueVals)})]\n\t\t[BooleanFalseValues({string.Join(',', falseVals)})]\n\t\t";
            }
            else
            {
                actualType = SupportedType.STRING;
            }

            return isEmpty ? $"{(string.IsNullOrWhiteSpace(attribute) ? "" : "//" + attribute)}{additionalAttributes}//public {actualType.Name()}{(hasNulls ? "?" : "")} {columnName}{(hasNulls ? "?" : "")} {{ get; set; }}" :
                              $"{attribute}{additionalAttributes}public {actualType.Name()}{(hasNulls ? "?" : "")} {columnName} {{ get; set; }}";
        }

        public static bool AllBoolValues(string[] values) => values.AsParallel().All(val => bool.TryParse(val, out bool b) || Bool_Values.Contains(val.ToLower()));

        public static bool AllDoubleValues(string[] values) => values.AsParallel().All(val => double.TryParse(val, out double d));

        public static bool AllIntValues(string[] values) => values.AsParallel().All(val => int.TryParse(val, out int d));

        public static bool AllDateTimeValues(string[] values) => values.AsParallel().All(val => DateTime.TryParse(val, out DateTime d));

        // add other types if you need...
    }

}
