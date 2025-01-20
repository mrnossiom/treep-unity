using Treep.Level;
using UnityEditor;
using UnityEngine;

namespace Treep.Editor {
    [CustomEditor(typeof(LevelAssembler))]
    class LevelAssemblerEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            var levelAssembler = (LevelAssembler)target;

            GUILayout.Space(15);

            if (GUILayout.Button("Generate Level")) {
                if (levelAssembler.GenerateLevel()) {
                    Debug.Log("done");
                }
                else {
                    Debug.Log("level is not solvable");
                }
            }

            if (GUILayout.Button("Clear")) {
                levelAssembler.Clear();
            }
        }
    }
}
