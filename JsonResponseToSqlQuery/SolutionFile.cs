using System;
using System.IO;
using Newtonsoft.Json;

namespace JsonResponseToSqlQuery
{
    internal class SolutionFile
    {
        internal Root JsonRoot { get; private init; }
        

        internal SolutionFile(string jsonResponseFile,
                string arrayName,
                string jsonVariableName,
                string defaultStringDataType,
                string defaultFloatDataType,
                string defaultDateDataType,
                string defaultIntegerDataType,
                string defaultUuidDataType,
                string innerArrayColumnNameSuffix,
                string queryAliasName,
                string sqlOutputFile,
                string overrideMappingFile
                )
        {
            JsonRoot = new Root
            {
                    DefaultResponseFileName = jsonResponseFile,
                    ArrayName = arrayName,
                    JsonVariableName = jsonVariableName,
                    QueryAliasName = queryAliasName,
                    SqlOutputFileName = sqlOutputFile,
                    MappingFileName = overrideMappingFile,
                    InnerArrayColumnNameSuffix = innerArrayColumnNameSuffix,
                    DefaultDataTypes = new DefaultDataTypes
                    {
                            StringDataType = defaultStringDataType,
                            FloatDateType = defaultFloatDataType,
                            DateDateType = defaultDateDataType,
                            IntegerDateType = defaultIntegerDataType,
                            UuidDateType = defaultUuidDataType
                    }
            };

        }

        private SolutionFile() { }

        internal static SolutionFile Load(FileInfo solutionFile)
        {
            return new SolutionFile() {JsonRoot = JsonConvert.DeserializeObject<Root>(solutionFile.ReadAllText())};
        }

        internal void Save(FileInfo solutionFile)
        {
            JsonRoot.MappingFileName = JsonRoot.MappingFileName.FileNameOnly();
            JsonRoot.DefaultResponseFileName = JsonRoot.DefaultResponseFileName.FileNameOnly();
            JsonRoot.SqlOutputFileName = JsonRoot.SqlOutputFileName.FileNameOnly();
            solutionFile.WriteAllText(JsonConvert.SerializeObject(JsonRoot, Formatting.Indented));
        }
        
        internal class Root
        {
            [JsonProperty("defaultResponseFileName")] internal string DefaultResponseFileName { get; set; }
            [JsonProperty("mappingFileName")] internal string MappingFileName { get; set; }
            [JsonProperty("arrayName")] internal string ArrayName { get; set; }
            [JsonProperty("jsonVariableName")] internal string JsonVariableName { get; set; }
            [JsonProperty("innerArrayColumnNameSuffix")] internal string InnerArrayColumnNameSuffix { get; set; }
            [JsonProperty("queryAliasName")] internal string QueryAliasName { get; set; }
            [JsonProperty("sqlOutputFileName")] internal string SqlOutputFileName { get; set; }
            
            [JsonProperty("defaultDataTypes")] internal DefaultDataTypes DefaultDataTypes { get; set; }
        }

        internal class DefaultDataTypes
        {
            [JsonProperty("stringDataType")] internal string StringDataType { get; set; }
            [JsonProperty("dateDataType")] internal string DateDateType  { get; set; }
            [JsonProperty("floatDataType")] internal string FloatDateType  { get; set; }
            [JsonProperty("integerDataType")] internal string IntegerDateType  { get; set; }
            [JsonProperty("uuidDataType")] internal string UuidDateType  { get; set; }
        }
        
    }
}