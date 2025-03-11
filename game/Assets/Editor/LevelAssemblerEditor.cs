using Treep.Level;
using UnityEditor;
using UnityEngine;

namespace Treep.Editor {
    [CustomEditor(typeof(LevelAssembler))]
    internal class LevelAssemblerEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            var levelAssembler = (LevelAssembler)this.target;

            GUILayout.Space(15);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate Level")) {
                if (levelAssembler.GenerateLevel(new System.Random().Next()) is null) {
                    Debug.LogError($"Level `{levelAssembler.name}` is not solvable!");
                }
            }

            if (GUILayout.Button("Clear")) {
                levelAssembler.ClearChildren();
            }

            GUILayout.EndHorizontal();
        }
    }
}
