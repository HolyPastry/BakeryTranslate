using System;
using System.Collections.Generic;

namespace Bakery.Translate
{
    public class CsvFile
    {
        public string Raw
        {
            get
            {
                List<string> lines = new();

                lines.Add(string.Join(",", Headers));
                foreach (var content in Content)
                {
                    lines.Add(string.Join(",", content));
                }
                return string.Join("\n", lines);
            }
            set
            {
                Content = new();
                string[] lines = value.Split("\n");

                for (var i = 0; i < lines.Length; i++)
                {
                    List<string> fieldList = ParseLine(lines[i]);
                    if (i == 0)
                        Headers = fieldList;
                    else
                        Content.Add(fieldList);
                }
            }
        }
        public List<string> Headers;
        public List<List<string>> Content;

        protected static List<string> ParseLine(string line)
        {
            int index = 0;
            List<string> stringList = new();

            while (index < line.Length)
            {
                if (line[index] == '"')
                {
                    int nextQuote = line.IndexOf('"', index + 1);
                    if (nextQuote == -1)
                        throw new Exception(
                            "Bad CSV Format (probably some return carriage in the story string)"
                        );
                    stringList.Add(line[(index + 1)..nextQuote]);
                    index = nextQuote + 2;
                    continue;
                }
                int nextComma = line.IndexOf(',', index + 1);
                if (line[index] == ',')
                {
                    stringList.Add("");
                    index++;
                    continue;
                }

                if (nextComma == -1)
                {
                    stringList.Add(line[index..]);
                    break;
                }
                stringList.Add(line[index..nextComma]);
                index = nextComma + 1;
            }

            return stringList;
        }
    }
}
