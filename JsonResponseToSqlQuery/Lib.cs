using System;
using System.IO;
using System.Linq;

namespace JsonResponseToSqlQuery
{
    internal static class Lib
    {
        internal static void Error(string message)
        {
            Console.Error.WriteLine(message);
        }
      
    }

    internal static class ExtensionMethods
    {
        internal static string FixCaseOfName(this string text, char sep = '.')
        {
            return !text.Contains(sep) ? text : string.Join(sep, (text.StartsWith(sep)?text.Substring(1):text).Split(sep).Select(e => e.Length == 1 ? e.ToUpper() : FixCaseOfName((e.Substring(0, 1).ToUpper() + e.Substring(1)), '_').FixCaseOfName('-')));
        }

        internal static string ReadAllText(this FileInfo file)
        {
            using var sr = file.OpenText();
            return sr.ReadToEnd();
        }

        internal static void WriteAllText(this FileInfo file, string text)
        {
            using var sw = file.CreateText();
            sw.Write(text);
            sw.Flush();
            sw.Close();
        }
    }
}