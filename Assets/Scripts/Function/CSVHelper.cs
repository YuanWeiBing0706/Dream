using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
namespace Function
{
    public static class CsvHelper
    {
        public static List<string[]> ReadCsv(TextAsset textAsset)
        {
            string[] lines = textAsset.text.Split(new[] { '\n' }, System.StringSplitOptions.None);

            List<string[]> result = new List<string[]>();
            for (int i = 1; i < lines.Length; i++)
            {
                var row = ParseLine(lines[i]);
                if (string.IsNullOrEmpty(row[0]) || string.IsNullOrWhiteSpace(row[0]))
                {
                    continue;
                }

                result.Add(row);
            }

            return result;
        }

        private static string[] ParseLine(string line)
        {
            List<string> result = new();
            bool inQuotes = false;
            StringBuilder currentValue = new();
            for (int i = 0; i < line.Length; i++)
            {
                char currentChar = line[i];
                if (currentChar == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (currentChar == ',' && !inQuotes)
                {
                    result.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(currentChar);
                }
            }

            var str = currentValue.ToString();
            str = Regex.Replace(str, @"[\r\n]", "");
            result.Add(str);
            return result.ToArray();
        }

        public static void Save(string path, string content)
        {
            var folder = Path.GetDirectoryName(path);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(path, content, Encoding.UTF8);
        }
    }
}
