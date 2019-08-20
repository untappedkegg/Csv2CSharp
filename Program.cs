using System;
using System.IO;

namespace Csv2SqlCli
{
    public static class Program
    {
        static int Main(string[] args)
        {

            // ToDo: Support more args/configuration

            if (args.Length == 0)
            {
                Console.WriteLine("Input File not specified");
                return -5;
            }

            var infileName = args[0];

            var cSharpClass = CsvToSql.CSharpClassCodeFromCsvFile(infileName, null, out string outfileName);
            
            File.WriteAllText($"{outfileName}.sql", cSharpClass);
            return 0;
        }
    }
}
