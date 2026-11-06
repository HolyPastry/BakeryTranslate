using System.Collections.Generic;
using UnityEngine;

namespace Bakery.Translate
{
    public struct CsvEntry
    {
        public string Comments;
        public string Context;

        public string Reference;

        public string MasterText;
        public string Table;
        public int MaxLength;

        public void UpdateFromSheet(List<string> fields, string sheetName)
        {
            Table = sheetName;
            Reference = $"{fields[0]}";
            MasterText = fields[1];
            MaxLength = string.IsNullOrEmpty(fields[2]) ? 0 : int.Parse(fields[2]);
            Context = fields[3];
            Comments = fields[4];
        }
    }
}
