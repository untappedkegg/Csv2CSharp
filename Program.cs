using System;
using System.IO;

namespace Csv2CSharpCli
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

                string classAttribute = string.Empty;
                string propertyAttribute = string.Empty;
                char delimiter = ',';

                try
                {
                    classAttribute = ConfigUtils.GetConfigValue("classAttribute");
                    propertyAttribute = ConfigUtils.GetConfigValue("propertyAttribute");
                    try
                    {
                        delimiter = ConfigUtils.GetConfigCharacter("delimiter");
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Invalid delimiter character");
                        return -10;
                    }
                } catch (Exception e)
                {
                    Console.WriteLine("An Error Occurred Reading the Configuration (Config.xml)");
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Continuing with default values");
                }

                if (delimiter == '\0')
                {
                    delimiter = ',';
                }

                var infileName = args[0];

                var cSharpClass = CsvToClass.CSharpClassCodeFromCsvFile(infileName, out string outfileName, delimiter, classAttribute, propertyAttribute);

                File.WriteAllText($"{outfileName}.cs", cSharpClass);
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
