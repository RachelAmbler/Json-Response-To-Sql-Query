using System;
using System.IO;
using System.Linq;

namespace JsonResponseToSqlQuery
{
    internal static class Lib
    {
        internal static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"\nError: {message}\n");
            Console.ResetColor();
        }
      
    }

    internal static class ExtensionMethods
    {
        internal static string FixCaseOfName(this string text, char sep = '.')
        {
            return !text.Contains(sep) ? text : string.Join(sep, (text.StartsWith(sep)?text.Substring(1):text).Split(sep).Select(e => e.Length == 1 ? e.ToUpper() : FixCaseOfName((e.Substring(0, 1).ToUpper() + e.Substring(1)), '_').FixCaseOfName('-')));
        }

        internal static FileInfo ConvertFilePathToFileInfo(this string text)
        {
            FileInfo ret = null;
            if (!string.IsNullOrEmpty(text))
                ret = new FileInfo(text);

            return ret;
        }
        
        internal static FileInfo ConvertFilePathToFileInfo(this string text,  FileInfo referencedFile)
        {
            FileInfo ret = null;
            if (!string.IsNullOrEmpty(text) && referencedFile != null)
                ret = new FileInfo(Path.Join(referencedFile.DirectoryName, text));

            return ret;
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

        
        internal static FileInfo ReplacePath(this FileInfo file, FileInfo referencedFile)
        {
            return file == null ? null : new FileInfo(Path.Join(Directory.GetDirectoryRoot(referencedFile.DirectoryName ?? string.Empty), file.Name));
        }

        internal static string FileNameOnly(this string fullFilenameAndPath)
        {
            if (string.IsNullOrEmpty(fullFilenameAndPath))
                return null;
            var fi = new FileInfo(fullFilenameAndPath);
            return fi.Name;
        }

        internal static string EnsurePathSsAbsolute(this string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            return "";

        }
        
    }
}