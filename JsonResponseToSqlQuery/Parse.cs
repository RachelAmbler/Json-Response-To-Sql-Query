using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static JsonResponseToSqlQuery.Lib;

namespace JsonResponseToSqlQuery
{

  internal class Parser
  {
    private const string INNER_ARRAY_TYPE = "NVarChar(MAX)";
    private const string INNER_ARRAY_TYPE_MARKER = "*" + INNER_ARRAY_TYPE;
    private SortedList<string, string> _dataTypes;
    private SortedList<int, string> _elementOrder;
    private SortedList<string, bool> _dataTypeIsArray;
    private SortedList<string, string> _parents;

    private int _elementNo = 0;
    private bool _rowOne = true;
    private readonly string _indent = "            ";
    private string _sql;
    private bool _atRoot = true;

    internal string Json { get; init; }

    internal string JsonVariableName { get; init; }

    internal string ArrayName { get; init; }

    internal string DefaultStringDataType { get; init; }

    internal string DefaultFloatDataType { get; init; }

    internal string DefaultDateDataType { get; init; }

    internal string DefaultIntegerDataType { get; init; }

    internal string DefaultUuidDataType { get; init; }

    internal string InnerArrayColumnNameSuffix { get; init; }

    internal string QueryAliasName { get; init; }

    //internal SortedList<string, string> Overrides{ get; init; }
    internal MapFile MapFile { get; init; }

    internal string HierarchySeparator { get; init; }


    internal Tuple<string, MapFile> ParseJsonResponse()
    {
      _dataTypes = new SortedList<string, string>();
      _elementOrder = new SortedList<int, string>();
      _dataTypeIsArray = new SortedList<string, bool>();
      _parents = new SortedList<string, string>();
      var buildMapFile = MapFile.MappedColumns.Count == 0 && MapFile.IgnoredColumns.Count == 0;

      var path = (ArrayName == "" ? "" : $", '$.{ArrayName}'");

      _sql = $@"
Select   *
  From   OpenJson({JsonVariableName}{path})
    With  (
";
      JToken node;
      try
      {
        node = JToken.Parse(Json);
      }
      catch (JsonReaderException jre)
      {
        Error($"Exception raised when trying to parse Response. {jre.Message}");
        return new Tuple<string, MapFile>(string.Empty, null);
      }

      RecursiveParseResponse(node, false, "");

      foreach (var thisElementNo in _elementOrder.Keys.OrderBy(e => e))
      {
        var elementName = _elementOrder[thisElementNo];
        var dataType = _dataTypes[elementName];

        var isArray = _dataTypeIsArray[elementName];

        if (dataType == "") continue;

        var parent = _parents.ContainsKey(elementName) ? _parents[elementName] + ".*" : "";
        elementName = "." + elementName;

        var localizedElementName = parent;
        if (!elementName.StartsWith(".")) localizedElementName = "." + elementName;

        // Check parent first (so we can splat whole hierarchies)

        if (MapFile.IgnoredColumns.Contains(localizedElementName))
          continue;

        /* 
         var result = CheckMapping(parent);
         if(result.Item1)
           continue;
         if (!string.IsNullOrEmpty(result.Item2))
           dataType = result.Item2;
         */

        localizedElementName = elementName;
        if (!elementName.StartsWith(".")) localizedElementName = "." + elementName;

        if (MapFile.IgnoredColumns.Contains(localizedElementName))
          continue;

        if (MapFile.MappedColumns.ContainsKey(localizedElementName))
          dataType = MapFile.MappedColumns[localizedElementName];

        /*
        result = CheckMapping(elementName);
        if(result.Item1)
          continue;
        if (!string.IsNullOrEmpty(result.Item2))
          dataType = result.Item2;
        */
        var columnName = "[" + (isArray ? (elementName + InnerArrayColumnNameSuffix).FixCaseOfName('.') : elementName.FixCaseOfName('.')) + "]";
        _sql += _indent + (_rowOne ? " " : ",") + columnName.Replace(".", HierarchySeparator).PadRight(96) + dataType.PadRight(24) + "'$" + elementName + "'" + (isArray ? " As Json" : "") + Environment.NewLine;
        if (buildMapFile)
          MapFile.MappedColumns.Add(elementName, dataType);
        //generatedOverrideMappingFileContents += $"{elementName.PadRight(64)} |>  {dataType}\n";
        _rowOne = false;
      }

      _sql += $@"          ) As {QueryAliasName};";

      var retMapFile = buildMapFile ? MapFile : null;

      return new Tuple<string, MapFile>(_sql, retMapFile);

      Tuple<bool, string> CheckMapping(string localizedElementName)
      {
        if (string.IsNullOrEmpty(localizedElementName))
          return new Tuple<bool, string>(false, string.Empty);

        if (!localizedElementName.StartsWith(".")) localizedElementName = "." + localizedElementName;

        if (MapFile.MappedColumns.ContainsKey(localizedElementName))
          return new Tuple<bool, string>(true, MapFile.MappedColumns[localizedElementName]);
        return MapFile.IgnoredColumns.Contains(localizedElementName) ? new Tuple<bool, string>(true, string.Empty) : new Tuple<bool, string>(false, string.Empty);

        // if (!Overrides.ContainsKey(localizedElementName)) return new Tuple<bool, string>(false, string.Empty);
        //var dataType = Overrides[localizedElementName];
        //return dataType == "*" ? new Tuple<bool, string>(true, string.Empty) : new Tuple<bool, string>(false, dataType);

      }
    }

    private void RecursiveParseResponse(JToken token, bool save = false, string parent = "")
    {
      if (_atRoot && ArrayName == "" && token is JArray array)
      {
        _atRoot = false;
        foreach (var t in array)
          RecursiveParseResponse(t, true, ArrayName);
        return;
      }

      switch (token)
      {
        case JProperty property:
        {
          var jProp = property;
          var name = parent == "" ? jProp.Name : $"{parent}.{jProp.Name}";
          if (!string.IsNullOrEmpty(parent) && !_parents.ContainsKey(name))
            _parents.Add(name, parent);

          if (save)
          {
            if (!_dataTypes.ContainsKey(name))
            {
              _dataTypes.Add(name, "");
              _elementOrder.Add(_elementNo++, name);
              _dataTypeIsArray.Add(name, false);
            }

            var savedDataType = _dataTypes[name];

            var jv = jProp.Value is JValue jValue ? jValue : null;
            var val = (jProp.Value is JValue value ? value.Value : "")?.ToString();
            var thisValueIsADateTime = DateTime.TryParse(val, out var valDt);

            var dataType = jv?.Type switch
            {
                JTokenType.Boolean => "Bit",
                JTokenType.Date when (savedDataType != DefaultStringDataType || savedDataType == "") => DefaultDateDataType,
                JTokenType.Integer => savedDataType == DefaultFloatDataType ? DefaultFloatDataType : DefaultIntegerDataType,
                JTokenType.String when thisValueIsADateTime => savedDataType != DefaultStringDataType ? DefaultDateDataType : DefaultStringDataType,
                JTokenType.String => DefaultStringDataType,
                JTokenType.Float => DefaultFloatDataType,
                JTokenType.Guid => DefaultUuidDataType,
                JTokenType.Object => DefaultStringDataType,
                JTokenType.Array => INNER_ARRAY_TYPE_MARKER,
                _ => null
            };

            if (jProp.Value.Type == JTokenType.Array)
              dataType = INNER_ARRAY_TYPE_MARKER;

            if (dataType == INNER_ARRAY_TYPE_MARKER)
            {
              dataType = INNER_ARRAY_TYPE;
              _dataTypeIsArray[name] = true;

            }

            if (dataType != null)
              _dataTypes[name] = dataType;
          }

          foreach (var child in jProp.Children())
          {
            RecursiveParseResponse(child, save, name);
          }

          break;
        }
        case JObject jObject:
        {

          foreach (var child in jObject.Children())
          {
            RecursiveParseResponse(child, save, parent == ArrayName ? "" : parent);
          }

          break;
        }
        case JArray jArray when jArray.First == null || !jArray.HasValues || !jArray.First.HasValues:
          return;
        case JArray jArray when jArray.Parent?.Path != ArrayName:
          return;
        case JArray jArray:
        {
          if (jArray.First != null)
            foreach (var t in jArray)
              RecursiveParseResponse(t, true, ArrayName);
          break;
        }
      }
    }
  }
}