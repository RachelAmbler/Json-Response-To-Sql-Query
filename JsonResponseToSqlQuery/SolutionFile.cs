using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using static JsonResponseToSqlQuery.Lib;

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
                string overrideMappingFile,
                string hierarchySeparator,
                string globalSolutionFile,
                SortedList<string, string> variables
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
                    HierarchySeparator = hierarchySeparator,
                    DefaultDataTypes = new DefaultDataTypes
                    {
                            StringDataType = defaultStringDataType,
                            FloatDateType = defaultFloatDataType,
                            DateDateType = defaultDateDataType,
                            IntegerDateType = defaultIntegerDataType,
                            UuidDateType = defaultUuidDataType
                    },
                    GlobalSolutionFile = globalSolutionFile,
                    Variables = variables
            };
            ReplaceVariables();

        }

        private SolutionFile() { }

        private void ReplaceVariables()
        {
            JsonRoot.DefaultResponseFileName = ReplaceVariable(JsonRoot.DefaultResponseFileName);
            JsonRoot.ArrayName = ReplaceVariable(JsonRoot.ArrayName);
            JsonRoot.JsonVariableName = ReplaceVariable(JsonRoot.JsonVariableName);
            JsonRoot.QueryAliasName = ReplaceVariable(JsonRoot.QueryAliasName);
            JsonRoot.SqlOutputFileName = ReplaceVariable(JsonRoot.SqlOutputFileName);
            JsonRoot.MappingFileName = ReplaceVariable(JsonRoot.MappingFileName);
            JsonRoot.InnerArrayColumnNameSuffix = ReplaceVariable(JsonRoot.InnerArrayColumnNameSuffix);
            JsonRoot.HierarchySeparator = ReplaceVariable(JsonRoot.HierarchySeparator);
            JsonRoot.DefaultDataTypes.StringDataType = ReplaceVariable(JsonRoot.DefaultDataTypes.StringDataType);
            JsonRoot.DefaultDataTypes.FloatDateType = ReplaceVariable(JsonRoot.DefaultDataTypes.FloatDateType);
            JsonRoot.DefaultDataTypes.DateDateType = ReplaceVariable(JsonRoot.DefaultDataTypes.DateDateType);
            JsonRoot.DefaultDataTypes.IntegerDateType = ReplaceVariable(JsonRoot.DefaultDataTypes.IntegerDateType);
            JsonRoot.DefaultDataTypes.UuidDateType = ReplaceVariable(JsonRoot.DefaultDataTypes.UuidDateType);
        }

        private string ReplaceVariable(string item)
        {
            if (string.IsNullOrEmpty(item))
                return item;
            foreach (var (variableName, variableValue) in JsonRoot.Variables)
            {
                item = item.Replace($"%%%{variableName}%%%", variableValue);
            }

            return item;
        }

        internal static SolutionFile Load(FileInfo solutionFile, bool isLocal = true)
        {
            var solution = new SolutionFile() {JsonRoot = JsonConvert.DeserializeObject<Root>(solutionFile.ReadAllText())};
            if (solution.JsonRoot == null)
            {
                Error($"Unable to process contents of the solution file '{solutionFile.FullName}");
                return null;
            }
            if (isLocal && !string.IsNullOrEmpty(solution.JsonRoot?.GlobalSolutionFile))
            {
                var globalSolutionFile = new FileInfo(solution.JsonRoot?.GlobalSolutionFile ?? string.Empty);
                if (!globalSolutionFile.Exists)
                {
                    Error($"Unable to read global solution file '{globalSolutionFile}");
                    return null;
                }
                var globalSolution = new SolutionFile() {JsonRoot = JsonConvert.DeserializeObject<Root>(globalSolutionFile.ReadAllText())};
                if (globalSolution.JsonRoot == null)
                {
                    Error($"Unable to process contents of the global solution file '{globalSolutionFile}");
                    return null;
                }
                
                solution.JsonRoot.ArrayName = PickValue(solution.JsonRoot.ArrayName, globalSolution.JsonRoot.ArrayName);
                solution.JsonRoot.HierarchySeparator = PickValue(solution.JsonRoot.HierarchySeparator, globalSolution.JsonRoot.HierarchySeparator);
                solution.JsonRoot.JsonVariableName = PickValue(solution.JsonRoot.JsonVariableName, globalSolution.JsonRoot.JsonVariableName);
                solution.JsonRoot.MappingFileName = PickValue(solution.JsonRoot.MappingFileName, globalSolution.JsonRoot.MappingFileName);
                solution.JsonRoot.QueryAliasName = PickValue(solution.JsonRoot.QueryAliasName, globalSolution.JsonRoot.QueryAliasName);
                solution.JsonRoot.DefaultResponseFileName = PickValue(solution.JsonRoot.DefaultResponseFileName, globalSolution.JsonRoot.DefaultResponseFileName);
                solution.JsonRoot.SqlOutputFileName = PickValue(solution.JsonRoot.SqlOutputFileName, globalSolution.JsonRoot.SqlOutputFileName);
                solution.JsonRoot.InnerArrayColumnNameSuffix = PickValue(solution.JsonRoot.InnerArrayColumnNameSuffix, globalSolution.JsonRoot.InnerArrayColumnNameSuffix);
                if (solution.JsonRoot.DefaultDataTypes != null)
                {
                    solution.JsonRoot.DefaultDataTypes.DateDateType = PickValue(solution.JsonRoot.DefaultDataTypes.DateDateType, globalSolution.JsonRoot.DefaultDataTypes.DateDateType);
                    solution.JsonRoot.DefaultDataTypes.FloatDateType = PickValue(solution.JsonRoot.DefaultDataTypes.FloatDateType, globalSolution.JsonRoot.DefaultDataTypes.FloatDateType);
                    solution.JsonRoot.DefaultDataTypes.IntegerDateType = PickValue(solution.JsonRoot.DefaultDataTypes.IntegerDateType, globalSolution.JsonRoot.DefaultDataTypes.IntegerDateType);
                    solution.JsonRoot.DefaultDataTypes.StringDataType = PickValue(solution.JsonRoot.DefaultDataTypes.StringDataType, globalSolution.JsonRoot.DefaultDataTypes.StringDataType);
                    solution.JsonRoot.DefaultDataTypes.UuidDateType = PickValue(solution.JsonRoot.DefaultDataTypes.UuidDateType, globalSolution.JsonRoot.DefaultDataTypes.UuidDateType);
                }
                else
                    solution.JsonRoot.DefaultDataTypes = globalSolution.JsonRoot.DefaultDataTypes;

                if (globalSolution.JsonRoot.Variables != null)
                {
                    solution.JsonRoot.Variables ??= new SortedList<string, string>();
                    foreach (var (variableName, variableValue) in globalSolution.JsonRoot.Variables)
                    {
                        if (!solution.JsonRoot.Variables.ContainsKey(variableName))
                            solution.JsonRoot.Variables.Add(variableName, variableValue);
                    }
                }

            }
            
            solution.ReplaceVariables();
            
            return solution;

            static string PickValue(string solutionEntry, string globalSolutionEntry)
            {
                solutionEntry = string.IsNullOrEmpty(solutionEntry) ? null : solutionEntry;
                globalSolutionEntry = string.IsNullOrEmpty(globalSolutionEntry) ? null : globalSolutionEntry;
                return solutionEntry ?? globalSolutionEntry;
            }
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
            [JsonProperty("hierarchySeparator")] internal string HierarchySeparator { get; set; }
            [JsonProperty("defaultDataTypes")] internal DefaultDataTypes DefaultDataTypes { get; set; }
            [JsonProperty("globalSolutionFile")] internal string GlobalSolutionFile { get; set; }
            [JsonProperty("variables")] internal SortedList<string, string> Variables { get; set; }
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