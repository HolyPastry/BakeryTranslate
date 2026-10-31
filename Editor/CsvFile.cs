using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bakery.Translate
{
    public class CsvFile
    {
        private const char quote = '\"';
        private const char endLine = '\n';
        private const char returnChar = '\r';
        private const char comma = ',';

        public List<string> Headers;
        public List<List<string>> Content;
        public string Raw
        {
            get
            {
                List<string> lines = new();

                lines.Add(string.Join(",", Headers));
                foreach (var content in Content)
                {
                    lines.Add("\"" + string.Join("\",\"", content) + "\"");
                }
                return string.Join("\n", lines);
            }
            set { parseRaw(value); }
        }

        private void parseRaw(string raw)
        {
            Content = new();
            bool insideHeaders = true;
            bool insideQuotes = false;
            bool insideQuotedField = false;

            string field = string.Empty;
            List<string> line = new();
            for (int i = 0; i < raw.Count() - 1; i++)
            {
                char c = raw[i];
                char cnext = raw[i + 1];

                if (insideQuotes)
                {
                    field += c;
                    if (c == quote && cnext == quote)
                    {
                        //field += cnext;
                        i++;
                        insideQuotes = false;
                    }

                    continue;
                }
                if (insideQuotedField)
                {
                    if (c != quote)
                    {
                        field += c;
                        continue;
                    }

                    if (c == quote && cnext == quote)
                    {
                        field += c;
                        insideQuotes = true;
                        i++;
                        continue;
                    }
                    insideQuotedField = false;
                    continue;
                }
                if (c == quote)
                {
                    if (cnext == quote && field.Count() > 1)
                    {
                        field += c;
                        i++;
                        insideQuotes = true;
                    }
                    else
                        insideQuotedField = true;
                    continue;
                }
                if (c == comma)
                {
                    line.Add(field.Trim());
                    field = string.Empty;
                    continue;
                }

                if (c == endLine)
                {
                    line.Add(field.Trim());
                    field = string.Empty;

                    if (insideHeaders)
                    {
                        Headers = line.ToList();
                        insideHeaders = false;
                    }
                    else
                        Content.Add(line.ToList());
                    line = new();
                    continue;
                }
                field += c;
            }
            line.Add(field.Trim());
            Content.Add(line.ToList());
            Debug.Log($"===End Parsing");
        }

        public override string ToString()
        {
            var str = String.Join(",", Headers);
            str += $"\n number of lines:{Content.Count}";

            return str;
        }

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
