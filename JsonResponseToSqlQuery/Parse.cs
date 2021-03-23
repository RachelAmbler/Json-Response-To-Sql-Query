using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace JsonResponseToSqlQuery
{
    internal class Parser
    {
        private SortedList<string, string> _dataTypes;
        private SortedList<int, string> _elementOrder;
        private SortedList<string, bool> _dataTypeIsArray;

        private int _elementNo = 0;
        private bool _rowOne = true;
        private readonly string _indent = "            ";
        private string _sql;
        
        internal string Json { get; init; }
        
        internal string JsonVariableName { get; init; }
        
        internal string ArrayName { get; init; }
        
        internal string DefaultStringDataType { get; init; }
        
        internal string DefaultFloatDataType { get; init; }
        
        internal string DefaultDateDataType { get; init; }
        
        internal string DefaultIntegerDataType { get; init; }
        
        internal string DefaultUuidDataType { get; init; }
        
        internal string DefaultInnerArrayDataType { get; init; }

        internal string InnerArrayColumnNameSuffix { get; init; }

        internal string QueryAliasName { get; init; }
        
        internal SortedList<string, string> Overrides{ get; init; }

        
        internal string ParseJsonReponse()
        {
            _dataTypes = new SortedList<string, string>();
            _elementOrder = new SortedList<int, string>();
            _dataTypeIsArray = new SortedList<string, bool>();
           
            _sql = $@"
Select   *
  From   OpenJson({JsonVariableName}, '$.{ArrayName}')
    With  (
";
            var node = JToken.Parse(Json);
        
            RecursiveParseResponse(node, false, "");

            foreach (var thisElementNo in _elementOrder.Keys.OrderBy(e => e))
            {
                var elementName = _elementOrder[thisElementNo];
                var dataType = _dataTypes[elementName];

                var isArray = _dataTypeIsArray[elementName];

                if (dataType == "") continue;
                
                elementName = "." + elementName;

                if (Overrides.ContainsKey(elementName))
                {
                  dataType = Overrides[elementName];
                  if(dataType == "*")
                    continue;
                }

                var columnName = "[" + (isArray ? (elementName + InnerArrayColumnNameSuffix).FixCaseOfName('.') : elementName.FixCaseOfName('.')) + "]";
                _sql += _indent + (_rowOne ? " " : ",") + columnName.PadRight(96) + dataType.PadRight(24) + "'$" + elementName + "'" + (isArray ? " As Json" : "") + Environment.NewLine;
                _rowOne = false;
            }
            _sql += $@"          ) As {QueryAliasName};";
            
            return _sql;
        }

        private void RecursiveParseResponse(JToken token, bool save = false, string parent = "")
        {
          switch (token)
          {
            case JProperty property:
            {
              var jProp = property;
              var name = parent == "" ? jProp.Name : $"{parent}.{jProp.Name}";

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
                    JTokenType.Integer =>  savedDataType == DefaultFloatDataType ? DefaultFloatDataType : DefaultIntegerDataType,
                    JTokenType.String when thisValueIsADateTime => savedDataType != DefaultStringDataType ? DefaultDateDataType : DefaultStringDataType,
                    JTokenType.String => DefaultStringDataType,
                    JTokenType.Float => DefaultFloatDataType,
                    JTokenType.Guid => DefaultUuidDataType,
                    JTokenType.Object => DefaultStringDataType,
                    JTokenType.Array => "*" + DefaultInnerArrayDataType,
                    _ => null
                };

                if (jProp.Value.Type == JTokenType.Array)
                  dataType = "*" + DefaultInnerArrayDataType;

                if (dataType == "*" + DefaultInnerArrayDataType)
                {
                  dataType = dataType.Substring(1);
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
                RecursiveParseResponse(child, save, parent == ArrayName? "":parent);
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