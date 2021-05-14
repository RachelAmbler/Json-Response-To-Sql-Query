using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Unicode;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace JsonResponseToSqlQuery
{
    internal class MapFile
    {
        public List<string> IncludeFiles;
        public SortedList<string, string> MappedColumns;
        public List<string> IgnoredColumns;

        public MapFile()
        {
            MappedColumns = new SortedList<string, string>();
            IgnoredColumns = new List<string>();
            IncludeFiles = new List<string>();
        }

        internal static MapFile Load(string fileContents)
        {
            var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .Build();
            try
            {
                return deserializer.Deserialize<MapFile>(fileContents);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        internal string GetYaml()
        {
            using var ms = new MemoryStream();
            using var sw = new StreamWriter(ms);
            var serializer = new SerializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .Build();
            serializer.Serialize(sw, this);
            sw.Flush();
            return Encoding.UTF8.GetString(ms.ToArray(), 0, (int) ms.Length);
        }
    }
    
    
}