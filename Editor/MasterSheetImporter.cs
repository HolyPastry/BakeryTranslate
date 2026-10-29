using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace Bakery.Translate
{
    [CreateAssetMenu(fileName = "TranslationController", menuName = "Bakery/TranslationController")]
    public class TranslationController : ScriptableObject
    {
        [SerializeField]
        private string _filenamePrefix = "po_";

        [SerializeField]
        private string _localizationTableFolder = "";

        [SerializeField]
        private string _exportPoFolder = "Assets/EditorTools/Localization/PoExports";

        [SerializeField]
        protected GoogleSheetAPI _googleSheetAPI;

        [SerializeField]
        protected List<string> _sheetList;

        public async Task Import()
        {
            EditorApplication.ExitPlaymode();

            Debug.Log("==================================");
            Debug.Log("    LOCALIZATION SYNC             ");

            List<PoEntry> masterTextEntries = await ImportMasterEntries();

            UpdateMasterLocalizationTable(masterTextEntries);
            UpdateLocales(masterTextEntries);
        }

        private string GetPoFilename(string localeCode) => _filenamePrefix + localeCode + ".po";

        private void UpdateLocales(List<PoEntry> masterTextEntries)
        {
            var locales = LocalizationEditorSettings.GetLocales();

            foreach (var local in locales)
            {
                if (local.Identifier.Code == "en") // skipping the master text language
                    continue;
                Debug.Log("=== Syncing Languages");
                Debug.Log($"==== Language: {local.Identifier.Code}");
                var filename = GetPoFilename(local.Identifier.Code);
                if (!FileManager.OpenFile(filename, _exportPoFolder, out string raw))
                {
                    CreateNewPoFile(local.Identifier.Code, filename, masterTextEntries);
                    continue;
                }
                ProcessPoFile(new(masterTextEntries), raw, local.Identifier.Code, filename);
            }
        }

        private async Task<List<PoEntry>> ImportMasterEntries()
        {
            List<PoEntry> masterTextEntries = new();
            Debug.Log("=== Syncing Master Text");

            foreach (var sheet in _sheetList)
            {
                var sheetEntries = await ImportSheet(sheet);
                masterTextEntries.AddRange(sheetEntries);
                Debug.Log($"==== Table: {sheet}");
            }
            return masterTextEntries;
        }

        private void UpdateMasterLocalizationTable(List<PoEntry> masterTextEntries)
        {
            foreach (var entry in masterTextEntries)
            {
                if (GetTable(entry, "en", out var table))
                    table.AddEntry(entry.Reference, entry.UntranslatedString);
            }
        }

        private bool GetTable(PoEntry entry, string localeCode, out StringTable table)
        {
            if (string.IsNullOrEmpty(entry.Table))
            {
                Debug.LogWarning($"Table Field is empty {entry.Reference}");
                table = null;
                return false;
            }

            var collection = LocalizationEditorSettings.GetStringTableCollection(entry.Table);
            if (collection == null)
                collection = LocalizationEditorSettings.CreateStringTableCollection(
                    entry.Table,
                    _localizationTableFolder
                );

            table = collection.GetTable(localeCode) as StringTable;

            if (table == null)
                table = collection.AddNewTable(localeCode) as StringTable;
            return true;
        }

        private void ProcessPoFile(
            List<PoEntry> masterEntries,
            string raw,
            string localeCode,
            string filename
        )
        {
            List<string> entries = new(raw.Split("\n\n"));
            List<PoEntry> poEntries = new();
            for (int i = 1; i < entries.Count; i++)
            {
                PoEntry newEntry = new();
                if (!newEntry.ParseEntry(entries[i]))
                    continue;
                poEntries.Add(newEntry);
            }
            List<PoEntry> exportEntries = new();

            //update or add all entries from the masterEntryList
            foreach (var entry in masterEntries)
            {
                if (!poEntries.Exists(e => e.Reference == entry.Reference))
                {
                    exportEntries.Add(entry);
                    continue;
                }
                var poEntry = poEntries.Find(e => e.Reference == entry.Reference);
                if (poEntry.UntranslatedString != entry.UntranslatedString)
                {
                    poEntry.PreviousUnTranslatedString = poEntry.UntranslatedString;
                    poEntry.Flag = "fuzzy";
                }
                poEntry.Table = entry.Table;
                poEntry.UntranslatedString = entry.UntranslatedString;
                poEntry.ExtractedComments = entry.ExtractedComments;

                exportEntries.Add(poEntry);
            }

            //Update the localization table
            foreach (var entry in exportEntries)
            {
                if (GetTable(entry, localeCode, out var table))
                    table.AddEntry(entry.Reference, entry.TranslatedString);
            }

            //Flag as Obsolete entries from the po files that are not in the Master List anymore
            List<PoEntry> deletedEntries = new();
            foreach (var entry in poEntries)
            {
                if (masterEntries.Exists(e => e.Reference == entry.Reference))
                    continue;
                //entry.SetObsolete(true);
                deletedEntries.Add(entry);
            }
            if (deletedEntries.Count > 0)
            {
                CreateNewPoFile(localeCode, "_deleted_" + filename, deletedEntries);
                Debug.LogWarning("WARNING - KEYS WERE DELETED OR MODIFIED");
            }
            // Update the PoFile
            CreateNewPoFile(localeCode, filename, exportEntries);
        }

        private string GetFileHeader(string languageCode)
        {
            var export = string.Empty;
            export += "msgid \"\"";
            export += "\nmsgstr \"\"";
            export += "\n\"Project-Id-Version: Good Folk Cafe \\n\"";
            export += $"\n\"Language: {languageCode}\\n\"";
            export += "\n\"MIME-Version: 1.0 \\n\"";
            export += "\n\"Content-Type: text/plain; charset=UTF-8 \\n\"";
            export += "\n\"Content-Transfer-Encoding: 8bit \\n\"";
            return export;
        }

        private void CreateNewPoFile(
            string languageCode,
            string filename,
            List<PoEntry> textEntries
        )
        {
            var export = GetFileHeader(languageCode);

            foreach (var entry in textEntries)
                export += entry.ExportString;

            FileManager.Create(filename, _exportPoFolder, export);
        }

        private async Task<List<PoEntry>> ImportSheet(string sheet)
        {
            List<List<string>> data = await _googleSheetAPI.Import(sheet);

            List<PoEntry> entries = new();
            for (int i = 1; i < data.Count; i++)
            {
                List<string> fields = data[i];
                if (fields.Count == 0 || string.IsNullOrEmpty(fields[0]))
                    continue;
                PoEntry entry = new();
                entry.UpdateFromSheet(fields, sheet);
                entries.Add(entry);
            }
            return entries;
        }

        internal void TestAccess()
        {
            _googleSheetAPI.TestGoogleAccess();
        }
    }
}
