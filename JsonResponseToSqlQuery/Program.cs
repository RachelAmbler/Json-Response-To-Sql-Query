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
        /// <param name="innerArrayColumnNameSuffix">The suffix to append to a column that contains a subarray [ System default = _JSON_ARRAY]</param>
        /// <param name="queryAliasName">The name to give to the alias for the json query [ System default = JsonQuery]</param>
        /// <param name="overrideMappingFile">Path to file containing any specific datatype override mappings</param>
        /// <param name="sqlOutputFile">Path to file that will contain the resulting Sql query. If not specified the the sql will be written to the console.</param>
        /// <param name="projectSolutionFile">Path to a project solution file.</param>
        /// <param name="projectSolutionFolder">Path to a project solution folder - there must be a single file ending with the extension .rsol in the folder .</param>
        /// <param name="createProjectSolutionFile">If a Project Solution file is specified, then passing this flag will force the app to create\overwrite the file based upon any values passed in the current command line [ System default = false].</param>
        /// <param name="autoCreateMappingFile">Auto create a Mapping file if one does not exist using the defaults gleamed from the Json response file.</param>
        /// <param name="hierarchySeparator">If passed this value will be used in the Sql column as opposed to the periods used by Json to define the hierarchy.</param>
        /// <param name="silent">If set then the app will not return any output other than exceptions [System default = false].</param>
        private static void Main(FileInfo jsonResponseFile = null,
                string arrayName = "",
                string jsonVariableName = "@Json",
                string defaultStringDataType = "NVarChar(4000)",
                string defaultFloatDataType = "Numeric(8, 4)",
                string defaultDateDataType = "DateTime2(7)",
                string defaultIntegerDataType = "BigInt",
                string defaultUuidDataType = "UniqueIdentifier",
                string innerArrayColumnNameSuffix = "_JSON_ARRAY",
                string queryAliasName = "JsonQuery",
                FileInfo sqlOutputFile = null,
                FileInfo overrideMappingFile = null,
                FileInfo projectSolutionFile = null,
                DirectoryInfo projectSolutionFolder = null,
                bool createProjectSolutionFile = false,
                bool autoCreateMappingFile = false,
                string hierarchySeparator = ".",
                bool silent = false
                )
        {
            var overrides = new SortedList<string, string>();

            if (projectSolutionFolder != null && !projectSolutionFolder.Exists)
            {
                Error($"Project Solution Folder '{projectSolutionFolder}' does not exist");
                return;
            }

            if (overrideMappingFile != null && autoCreateMappingFile && overrideMappingFile.Exists)
            {
                Error($"Existing override mapping file '{overrideMappingFile}' already exists and auto-create-mapping-file was set");
                return;
            }

            if (projectSolutionFolder != null)
            {
                var solutionFiles = projectSolutionFolder.GetFiles("*.rsol");
                switch (solutionFiles.Length)
                {
                    case 0:
                        Error($"Unable to find any solution files in the specified folder '{projectSolutionFolder}'");
                        return;
                    case > 1:
                        Error($"Multiple solution files found in '{projectSolutionFolder}'. Please remove extra files or use the --project-Solution-File parameter instead");
                        return;
                    default:
                        projectSolutionFile = solutionFiles[0];
                        break;
                }
            }

            if (projectSolutionFile != null && !projectSolutionFile.Name.EndsWith("rsol"))
            {
                Error($"Project solution filename has the incorrect extension - it should end with .rsol");
                return;
            }

            if (jsonResponseFile == null && projectSolutionFile == null && !createProjectSolutionFile)
            {
                var files = Directory.GetFiles(".", "rsol");
                if (files.Length == 1)
                    projectSolutionFile = files[0].ConvertFilePathToFileInfo();
            }

            if (projectSolutionFile != null)
            {
                sqlOutputFile = sqlOutputFile.ReplacePath(projectSolutionFile);
                overrideMappingFile = overrideMappingFile.ReplacePath(projectSolutionFile);
                
                if (!createProjectSolutionFile && !projectSolutionFile.Exists)
                {
                    Error($"Unable to locate Project solution file '{projectSolutionFile.FullName}'");
                    return;
                }

                if (projectSolutionFile?.Directory != null && !projectSolutionFile.Directory.Exists)
                {
                    Error($"Unable to locate the folder for the Project Solution file '{projectSolutionFile.FullName}'");
                    return;
                }

                if (createProjectSolutionFile)
                {
                    
                    var solutionFile = new SolutionFile(jsonResponseFile == null? string.Empty: jsonResponseFile.FullName,
                            arrayName,
                            jsonVariableName,
                            defaultStringDataType,
                            defaultFloatDataType,
                            defaultDateDataType,
                            defaultIntegerDataType,
                            defaultUuidDataType,
                            innerArrayColumnNameSuffix,
                            queryAliasName,
                            sqlOutputFile == null ? string.Empty:sqlOutputFile.FullName,
                            overrideMappingFile == null ? string.Empty: overrideMappingFile.FullName,
                            hierarchySeparator);
                    
                    solutionFile.Save(projectSolutionFile);
                }
                else
                {
                    var solutionFile = SolutionFile.Load(projectSolutionFile);
                    
                    jsonResponseFile ??= solutionFile.JsonRoot.DefaultResponseFileName.ConvertFilePathToFileInfo(projectSolutionFile);
                    arrayName = solutionFile.JsonRoot.ArrayName;
                    jsonVariableName = solutionFile.JsonRoot.JsonVariableName;
                    innerArrayColumnNameSuffix = solutionFile.JsonRoot.InnerArrayColumnNameSuffix;
                    queryAliasName = solutionFile.JsonRoot.QueryAliasName;
                    sqlOutputFile = solutionFile.JsonRoot.SqlOutputFileName.ConvertFilePathToFileInfo(projectSolutionFile);
                    overrideMappingFile = solutionFile.JsonRoot.MappingFileName.ConvertFilePathToFileInfo(projectSolutionFile);
                    hierarchySeparator = solutionFile.JsonRoot.HierarchySeparator;
                    
                    defaultDateDataType = solutionFile.JsonRoot.DefaultDataTypes.DateDateType;
                    defaultFloatDataType = solutionFile.JsonRoot.DefaultDataTypes.FloatDateType;
                    defaultIntegerDataType = solutionFile.JsonRoot.DefaultDataTypes.IntegerDateType;
                    defaultStringDataType = solutionFile.JsonRoot.DefaultDataTypes.StringDataType;
                    defaultUuidDataType = solutionFile.JsonRoot.DefaultDataTypes.UuidDateType;
                    
                    if (overrideMappingFile != null && autoCreateMappingFile && overrideMappingFile.Exists)
                        autoCreateMappingFile = false;

                }
                
            }
                
            if (jsonResponseFile != null && !jsonResponseFile.Exists)
            {
                Error($"Unable to locate Json Response file '{jsonResponseFile.FullName}'");
                return;
            }

            if (sqlOutputFile?.Directory != null && !sqlOutputFile.Directory.Exists)
            {
                Error($"Unable to locate the folder requested for the the Sql output '{sqlOutputFile.DirectoryName}'");
                return;
            }

            var json = jsonResponseFile.ReadAllText();
            if (overrideMappingFile != null && !autoCreateMappingFile)
            {
                if (!overrideMappingFile.Exists)
                {
                    Error($"Unable to locate override mapping file '{overrideMappingFile.FullName}'");
                    return;
                }

                if (ParseOverrideFile(overrideMappingFile, 0) == false)
                    return;
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
                    InnerArrayColumnNameSuffix = innerArrayColumnNameSuffix,
                    QueryAliasName = queryAliasName,
                    Overrides = overrides,
                    HierarchySeparator = hierarchySeparator
            };

            var (sql, generatedOverrideMappingFileContents) = parser.ParseJsonResponse();

            if (overrideMappingFile != null && autoCreateMappingFile)
            {
                overrideMappingFile.WriteAllText(generatedOverrideMappingFileContents);
                if(!silent)
                    Console.WriteLine($"\n\nOverride mapping file {overrideMappingFile.FullName} created\n");
            }

            switch (sqlOutputFile)
            {
                case null:
                    Console.WriteLine(sql);
                    break;
                default:
                    sqlOutputFile.WriteAllText(sql);
                    if (!silent)
                        Console.WriteLine($"\n\nSql written to {sqlOutputFile.FullName}\n");
                    break;
            }

            bool ParseOverrideFile(FileInfo thisOverrideMappingFile, int n)
            {
                if (!thisOverrideMappingFile.Exists)
                {
                    Error($"Unable to locate primary override mapping file '{thisOverrideMappingFile.FullName}");
                    return false;
                }
                var contents = thisOverrideMappingFile.ReadAllText();
                
                var fail = new Tuple<bool, string>(false, string.Empty);
                if (n == 10)
                {
                    Error("Too many nested override mapping files provided. Please ensure there are no more than 10 nested files and try again'");
                    return false;
                }
                foreach (var includeRef in contents.Split(Environment.NewLine).Where(l => l.StartsWith("# *include:")).Select(i => i.Remove(0, 11).Trim()))
                {
                    var includeRefFullPath = Path.GetFullPath(overrideMappingFile.DirectoryName + "/" + includeRef);
                    var includedFile = new FileInfo(includeRefFullPath);
                    if (!includedFile.Exists)
                    {
                        Error($"Unable to locate included file '{includeRefFullPath}' defined in {thisOverrideMappingFile.FullName}");
                        return false;
                    }
                    var ret = ParseOverrideFile(includedFile, n + 1);
                    if (ret == false)
                        return false;
                }
                
                foreach (var def in contents.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(l => l.Split("|>")))
                {
                    if(def.Length < 1 || def[0].StartsWith("#"))
                        continue;
                    var propertyName = def[0].Trim();
                    var overrideValue = def[1].Trim();
                    
                    if(overrides.ContainsKey(propertyName))
                        overrides[propertyName] = overrideValue;
                    else
                        overrides.Add(propertyName, overrideValue);
                }

                return true;
            }
        }
    }
}