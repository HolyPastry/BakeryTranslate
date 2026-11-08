using UnityEditor;
using UnityEngine;

namespace Bakery.Translate
{
    [CustomEditor(typeof(TranslationController))]
    public class MasterSheetImporterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Test Access To Google Sheet"))
            {
                ((TranslationController)target).TestAccess();
            }
            if (GUILayout.Button("Import Master Text"))
            {
                _ = ((TranslationController)target).Import();
            }

            if (GUILayout.Button("Export Csv Tables"))
            {
                ((TranslationController)target).ExportCsvTables();
            }
        }
    }
}
