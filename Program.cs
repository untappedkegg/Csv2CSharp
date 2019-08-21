using System;
using System.IO;

namespace Csv2SqlCli
{
    public static class Program
    {
        static int Main(string[] args)
        {
            try
            {
                // ToDo: Support more args/configuration
                if (args.Length == 0)
                {
                    Console.WriteLine("Input File not specified");
                    return -5;
                }


                var schema = ConfigUtils.GetConfigValue("schema");
                char delimiter;
                try
                {
                    delimiter = ConfigUtils.GetConfigCharacter("delimiter");
                }
                catch (Exception)
                {
                    Console.WriteLine("Invalid delimiter character");
                    return -10;
                }

                if (delimiter == '\0')
                {
                    delimiter = ',';
                }

                var infileName = args[0];

                var cSharpClass = CsvToSql.SqlTableFromCsvFile(infileName, schema, out string outfileName, delimiter);

                File.WriteAllText($"{outfileName}.sql", cSharpClass);
                return 0;
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
                Console.Read();
                return -2;
            }
        }
    }
}
