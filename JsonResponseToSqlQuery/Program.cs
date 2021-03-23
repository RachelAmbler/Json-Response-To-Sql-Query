using System;
using System.Collections.Generic;
using System.CommandLine.DragonFruit;
using System.IO;
using System.Linq;
using static JsonResponseToSqlQuery.Lib;

namespace JsonResponseToSqlQuery
{
    internal class Program
    {
        
        /// <param name="jsonResponseFile">Path to file containing the Json Response to parse</param>
        /// <param name="arrayName">The name of the array inside the Json Response to create the Select statement for</param>
        /// <param name="jsonVariableName">The name of the Json Variable or column to embed inside the Select statement [System default = @Json]</param>
        /// <param name="defaultStringDataType">The Sql Datatype to default all strings to [ System default = NVarChar(4000) ]</param>
        /// <param name="defaultFloatDataType">The Sql Datatype to default all Float values to [System default = Numeric(8, 4)]</param>
        /// <param name="defaultDateDataType">AThe Sql Datatype to default all DateTime values to [System default = DateTime2(7)]</param>
        /// <param name="defaultIntegerDataType">The Sql Datatype to default all Integer values to [System default = BigInt]</param>
        /// <param name="defaultUuidDataType">The Sql Datatype to default all Uuid values to [System default = UniqueIdentifier]</param>
        /// <param name="defaultInnerArrayDataType">The Sql Datatype to default all Inner Array values to [System default = NVarChar(4000)]</param>
        /// <param name="innerArrayColumnNameSuffix">The suffix to append to a column that contains a subarray [ System default = _JSON_ARRAY]</param>
        /// <param name="queryAliasName">The name to give to the alias for the json query [ System default = JsonQuery]</param>
        /// <param name="overrideMappingFile">Path to file containing any specific datatype override mappings</param>
        /// <param name="sqlOutputFile">Path to file that will contain the resulting Sql query. If not specified the the sql will be written to the console.</param>
        private static void Main(FileInfo jsonResponseFile,
                string arrayName,
                string jsonVariableName = "@Json",
                string defaultStringDataType = "NVarChar(4000)",
                string defaultFloatDataType = "Numeric(8, 4)",
                string defaultDateDataType = "DateTime2(7)",
                string defaultIntegerDataType = "BigInt",
                string defaultUuidDataType = "UniqueIdentifier", 
                string defaultInnerArrayDataType = "NVarChar(4000)",
                string innerArrayColumnNameSuffix = "_JSON_ARRAY",
                string queryAliasName = "JsonQuery",
                FileInfo sqlOutputFile = null,
                FileInfo overrideMappingFile = null)
        {
            var overrides = new SortedList<string, string>();
                
            if (!jsonResponseFile.Exists)
            {
                Error($"Unable to locate Json Response file {jsonResponseFile.FullName}");
                return;
            }

            if (sqlOutputFile?.Directory != null && !sqlOutputFile.Directory.Exists)
            {
                Error($"Unable to locate the folder requested for the the Sql output {sqlOutputFile.DirectoryName}");
                return;
            }

            var json = jsonResponseFile.ReadAllText();
            if (overrideMappingFile != null)
            {
                if (!overrideMappingFile.Exists)
                {
                    Error($"Unable to locate override mapping file {overrideMappingFile.FullName}");
                    return;
                }

                var overrideContents = overrideMappingFile.ReadAllText();
                
                foreach (var def in overrideContents.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(l => l.Split("|>")))
                {
                    if(def.Length < 1 || def[0].StartsWith("#"))
                        continue;
                    overrides.Add(def[0].Trim(), def[1].Trim());
                }
            }

            var parser = new Parser
            {
                    Json = json,
                    JsonVariableName = jsonVariableName,
                    ArrayName = arrayName,
                    DefaultStringDataType = defaultStringDataType,
                    DefaultFloatDataType = defaultFloatDataType,
                    DefaultDateDataType = defaultDateDataType,
                    DefaultIntegerDataType = defaultIntegerDataType,
                    DefaultUuidDataType = defaultUuidDataType,
                    DefaultInnerArrayDataType = defaultInnerArrayDataType,
                    InnerArrayColumnNameSuffix = innerArrayColumnNameSuffix,
                    QueryAliasName = queryAliasName,
                    Overrides = overrides
            };

            var sql = parser.ParseJsonReponse();

            switch (sqlOutputFile)
            {
                case null:
                    Console.WriteLine(sql);
                    break;
                default:
                    Console.WriteLine($"Sql written to {sqlOutputFile.FullName}");
                    sqlOutputFile.WriteAllText(sql);
                    break;
            }
        }
    }
}