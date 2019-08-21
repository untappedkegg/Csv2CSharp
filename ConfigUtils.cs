using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Csv2SqlCli
{
    internal static class ConfigUtils
    {
        public static string GetConfigValue(string attrName)
        {
            return GetNode(attrName).InnerText;
        }

        public static bool GetConfigBool(string attrName)
        {
            return bool.Parse(GetNode(attrName).InnerText);
        }

        public static string[] GetConfigValues(string attrName)
        {
            var children = GetNode(attrName).ChildNodes;
            string[] retVal = new string[children.Count];

            for (int i = 0; i < children.Count; i++)
            {
                retVal[i] = children[i].InnerText;
            }

            return retVal;
        }

        internal static char GetConfigCharacter(string attrName)
        {
            var nodeString = GetNode(attrName).InnerText;

            return nodeString switch
            {
                "NEWLINE" => '\n',
                "SPACE" => ' ',
                "TAB" => '\t',
                _ => string.IsNullOrEmpty(nodeString) ? '\0' : char.Parse(nodeString),
            };
        }

        private static XmlNode GetNode(string attrName)
        {
            var xmlReader = new XmlDocument();
            xmlReader.Load(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "Config.xml");
            return xmlReader.SelectSingleNode("ConfigOptions").SelectSingleNode(attrName);
        }

        internal static int GetConfigInt(string attrName)
        {
            return int.Parse(GetNode(attrName).InnerText);
        }
    }
}
