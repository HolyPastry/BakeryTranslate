using System;
//using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
//using UnityEditor.Localization.Plugins.CSV;
using UnityEngine;

namespace Bakery.Translate
{
    [Serializable]
    public class GoogleSheetAPI
    {
        public string GoolgleSheetID;

        public const string URLFormat =
            @"https://docs.google.com/spreadsheets/d/{0}/gviz/tq?tqx=out:csv&sheet={1}";

        public string URL(string sheetName) => string.Format(URLFormat, GoolgleSheetID, sheetName);

        public async void TestGoogleAccess()
        {
            CsvFile testResult = await DownloadCSV("");

            Debug.Log(testResult);
        }

        public async Task<CsvFile> DownloadCSV(string sheetName)
        {
            var webClient = new WebClient();

            Task<string> request;

            try
            {
                request = webClient.DownloadStringTaskAsync(URL(sheetName));
            }
            catch (WebException)
            {
                throw new Exception($"Bad URL '{URL(sheetName)}'");
            }

            while (!request.IsCompleted)
                await Task.Delay(100);

            try
            {
                CsvFile csv = new() { Raw = request.Result };

                request.Dispose();
                webClient.Dispose();

                return csv;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
                throw new Exception($"CSV line parsing failed");
            }
        }

        // public async Task<List<List<string>>> Import(string sheetName)
        // {
        //     try
        //     {
        //         string[] lines = await DownloadCSV(sheetName);
        //         List<List<string>> data = new();

        //         for (var i = 0; i < lines.Length; i++)
        //         {
        //             List<string> fieldList = ParseCsvString(lines[i]);
        //             data.Add(fieldList);
        //         }
        //         return data;
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.LogWarning($"Import Failed: {sheetName}");
        //         Debug.LogWarning(e.Message);
        //     }
        //     return null;
        // }

        // protected static List<string> ParseCsvString(string line)
        // {
        //     int index = 0;
        //     List<string> stringList = new();

        //     while (index < line.Length)
        //     {
        //         if (line[index] == '"')
        //         {
        //             int nextQuote = line.IndexOf('"', index + 1);
        //             if (nextQuote == -1)
        //                 throw new Exception(
        //                     "Bad CSV Format (probably some return carriage in the story string)"
        //                 );
        //             stringList.Add(line[(index + 1)..nextQuote]);
        //             index = nextQuote + 2;
        //             continue;
        //         }
        //         int nextComma = line.IndexOf(',', index + 1);
        //         if (line[index] == ',')
        //         {
        //             stringList.Add("");
        //             index++;
        //             continue;
        //         }

        //         if (nextComma == -1)
        //         {
        //             stringList.Add(line[index..]);
        //             break;
        //         }
        //         stringList.Add(line[index..nextComma]);
        //         index = nextComma + 1;
        //     }

        //     return stringList;
        // }
    }
}
