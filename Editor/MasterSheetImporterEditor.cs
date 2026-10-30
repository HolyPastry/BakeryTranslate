using UnityEditor;
using UnityEngine;

namespace Bakery.Translate
{
    [CustomEditor(typeof(TranslationController))]
    public class MasterSheetImporterEditor : Editor
    {
        private bool importing = false;

        public override async void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Test Access To Google Sheet"))
            {
                ((TranslationController)target).TestAccess();
            }
            if (GUILayout.Button("Sync Localization"))
            {
                if (importing)
                {
                    Debug.LogWarning("Import Process already in progress");
                    return;
                }
                importing = true;
                await ((TranslationController)target).Import();
                importing = false;
            }
        }
    }
}
