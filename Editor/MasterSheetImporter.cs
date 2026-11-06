using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.CSV;
using UnityEditor.Localization.Plugins.CSV.Columns;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace Bakery.Translate
{
    [CreateAssetMenu(fileName = "TranslationController", menuName = "Bakery/TranslationController")]
    public class TranslationController : ScriptableObject
    {
        [SerializeField]
        private string _localizationTableFolder = "";

        [SerializeField]
        private string _csvExportPath = "Assets/Localization/Exports";

        [SerializeField]
        protected GoogleSheetAPI _googleSheetAPI;

        [SerializeField]
        protected List<string> _sheetList;

        public async Task Import()
        {
            EditorApplication.ExitPlaymode();

            Debug.Log("==================================");
            Debug.Log("    LOCALIZATION SYNC             ");

            List<CsvEntry> masterTextEntries = await ImportMasterEntries();
            UpdateMasterLocalizationTable(masterTextEntries);
        }

        private async Task<List<CsvEntry>> ImportMasterEntries()
        {
            List<CsvEntry> masterTextEntries = new();
            Debug.Log("=== Syncing Master Text");

            foreach (var sheet in _sheetList)
            {
                var sheetEntries = await ImportSheet(sheet);
                masterTextEntries.AddRange(sheetEntries);
                Debug.Log($"==== Table: {sheet}");
            }
            return masterTextEntries;
        }

        private void UpdateMasterLocalizationTable(List<CsvEntry> masterTextEntries)
        {
            foreach (var csvEntry in masterTextEntries)
            {
                if (GetTable(csvEntry, "en", out var table))
                {
                    var olderEntry = table.GetEntry(csvEntry.Reference);
                    bool isFuzzy =
                        olderEntry == null
                        || (olderEntry != null && olderEntry.LocalizedValue != csvEntry.MasterText);

                    var entry = table.AddEntry(csvEntry.Reference, csvEntry.MasterText);
                    var metaData = entry.GetMetadata<BakeryMetaData>();

                    if (metaData == null)
                    {
                        metaData = new();
                        entry.AddMetadata(metaData);
                    }

                    metaData.Context = csvEntry.Context;
                    metaData.MaxLength = csvEntry.MaxLength;
                    metaData.CommentToTranslator = csvEntry.Comments;
                    metaData.Scope = csvEntry.Table;
                    metaData.IsFuzzy = isFuzzy;
                }
            }
        }

        private bool GetTable(CsvEntry entry, string localeCode, out StringTable table)
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

        private async Task<List<CsvEntry>> ImportSheet(string sheet)
        {
            CsvFile csv = await _googleSheetAPI.DownloadCSV(sheet);

            List<CsvEntry> entries = new();
            foreach (var fields in csv.Content)
            {
                if (fields.Count == 0 || string.IsNullOrEmpty(fields[0]))
                    continue;
                CsvEntry entry = new();
                entry.UpdateFromSheet(fields, sheet);
                entries.Add(entry);
            }
            return entries;
        }

        internal void TestAccess()
        {
            _googleSheetAPI.TestGoogleAccess();
        }

        public void ExportCsvTables()
        {
            var locales = LocalizationEditorSettings.GetLocales();

            string[] assetFiles = Directory.GetFiles(_localizationTableFolder, "*.asset");

            foreach (var local in locales)
            {
                ExportLocalCsvTables(assetFiles, local);
            }
        }

        private void ExportLocalCsvTables(
            string[] assetFiles,
            UnityEngine.Localization.Locale local
        )
        {
            var code = local.Identifier.Code;
            if (code == "en")
                return;
            var localPath = Path.Combine(_csvExportPath, code);
            //Directory.CreateDirectory(localPath);

            foreach (var assetPath in assetFiles)
            {
                var collection = AssetDatabase.LoadAssetAtPath<StringTableCollection>(assetPath);
                if (collection == null)
                    continue;

                var columnMappings = new List<CsvColumns>();

                columnMappings.Add(
                    new KeyIdColumns { IncludeSharedComments = false, IncludeId = false }
                );

                columnMappings.Add(
                    new LocaleColumns { LocaleIdentifier = "en", IncludeComments = false }
                );

                columnMappings.Add(
                    new LocaleColumns { LocaleIdentifier = code, IncludeComments = false }
                );
                columnMappings.Add(new CsvBakeryColumns { LocaleIdentifier = "en" });

                string exportPath = Path.Combine(localPath, $"{code} - {collection.name}.csv");
                using (var stream = new StreamWriter(exportPath, false, Encoding.UTF8))
                {
                    Csv.Export(stream, collection, columnMappings);
                }

                Debug.Log($"Successfully exported CSV with custom columns to: {exportPath}");
            }
        }
    }
}
