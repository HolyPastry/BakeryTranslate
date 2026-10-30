using UnityEditor;
using UnityEngine;

namespace Bakery.Translate
{
    [CustomEditor(typeof(TranslationController))]
    public class MasterSheetImporterEditor : Editor
    {
        public override async void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Test Access To Google Sheet"))
            {
                ((TranslationController)target).TestAccess();
            }
            if (GUILayout.Button("Sync Localization"))
            {
                await ((TranslationController)target).Import();
            }
        }
    }
}
