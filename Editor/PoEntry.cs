using System.Collections.Generic;
using UnityEngine;

namespace Bakery.Translate
{
    public struct PoEntry
    {
        public string TranslatorComments;
        public string ExtractedComments;

        public string Reference;
        public string Flag;
        public string PreviousUnTranslatedString;

        public string UntranslatedString;
        public string TranslatedString;

        public string Table;

        public List<string> ObsoleteList;

        public bool Obsolete;

        public void UpdateFromSheet(List<string> fields, string sheetName)
        {
            Obsolete = false;
            Table = sheetName;
            Reference = fields[0];
            UntranslatedString = fields[1];
            ExtractedComments = string.Empty;
            ExtractedComments += $"Scope: {sheetName}, ";
            ExtractedComments += $"MaxLen: {fields[2]},";
            if (!string.IsNullOrEmpty(fields[3]))
                ExtractedComments += $"Context: {fields[3]},";
            if (!string.IsNullOrEmpty(fields[4]))
                ExtractedComments += $"Comments: {fields[4]},";
        }

        public bool ParseEntry(string entry)
        {
            ObsoleteList = new();
            List<string> lines = new(entry.Split("\n"));
            foreach (var line in lines)
            {
                if (line.StartsWith("#~ "))
                {
                    ObsoleteList.Add(line);
                    Obsolete = true;
                    continue;
                }

                if (line.StartsWith("msgid "))
                {
                    UntranslatedString = line[6..].Trim('\"');
                    continue;
                }

                if (line.StartsWith("msgctxt "))
                {
                    ExtractedComments = line[8..].Trim('\"');
                    continue;
                }
                if (line.StartsWith("#: "))
                {
                    Reference = line[3..].Trim('\"');
                    continue;
                }
                if (line.StartsWith("#, "))
                {
                    Flag = line[3..].Trim('\"');
                    continue;
                }
                if (line.StartsWith("#| "))
                {
                    PreviousUnTranslatedString = line[3..].Trim('\"');
                    continue;
                }

                if (line.StartsWith("msgstr "))
                {
                    TranslatedString = line[7..].Trim('\"');
                    continue;
                }

                if (line.StartsWith("# "))
                {
                    TranslatorComments += line[3..].Trim('\"');
                    continue;
                }
                Debug.LogWarning("ParseEntry - Unable to Parse: " + line);
                return false;
            }
            return true;
        }

        public readonly string ExportString
        {
            get
            {
                string exportString = string.Empty;
                if (!Obsolete)
                {
                    if (!string.IsNullOrEmpty(TranslatorComments))
                        exportString += $"\n#  {TranslatorComments}";

                    if (!string.IsNullOrEmpty(Reference))
                        exportString += $"\n#: {Reference}";

                    if (!string.IsNullOrEmpty(Flag))
                        exportString += $"\n#, {Flag}";

                    if (!string.IsNullOrEmpty(PreviousUnTranslatedString))
                        exportString += $"\n#| {PreviousUnTranslatedString}";

                    if (!string.IsNullOrEmpty(ExtractedComments))
                        exportString += $"\nmsgctxt \"{ExtractedComments}\"";

                    if (!string.IsNullOrEmpty(UntranslatedString))
                        exportString += $"\nmsgid \"{UntranslatedString}\"";
                    // if (!string.IsNullOrEmpty(TranslatedString))
                    exportString += $"\nmsgstr \"{TranslatedString}\"";
                }
                else
                {
                    if (!string.IsNullOrEmpty(TranslatorComments))
                        exportString += $"\n#~  {TranslatorComments}";

                    if (!string.IsNullOrEmpty(Reference))
                        exportString += $"\n#~ {Reference}";

                    if (!string.IsNullOrEmpty(Flag))
                        exportString += $"\n#~ {Flag}";

                    if (!string.IsNullOrEmpty(ExtractedComments))
                        exportString += $"\n#~ msgctxt \"{ExtractedComments}\"";

                    if (!string.IsNullOrEmpty(UntranslatedString))
                        exportString += $"\n#~ msgid \"{UntranslatedString}\"";
                    // if (!string.IsNullOrEmpty(TranslatedString))
                    if (!string.IsNullOrEmpty(TranslatedString))
                        exportString += $"\n#~ msgstr \"{TranslatedString}\"";
                    if (ObsoleteList != null)
                        foreach (var line in ObsoleteList)
                            exportString += $"\n{line}";
                }
                if (!string.IsNullOrEmpty(exportString))
                    exportString = $"\n{exportString}";
                return exportString;
            }
        }

        internal void SetObsolete(bool v)
        {
            Obsolete = v;
        }
    }
}
