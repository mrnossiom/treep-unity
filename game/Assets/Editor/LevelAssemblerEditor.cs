using Treep.Level;
using UnityEditor;
using UnityEngine;

namespace Treep.Editor {
    [CustomEditor(typeof(LevelAssembler))]
    internal class LevelAssemblerEditor : UnityEditor.Editor {
        private int _seed = 0;

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            var levelAssembler = (LevelAssembler)this.target;

            GUILayout.Space(15);


            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate Level")) {
                this._seed = new System.Random().Next();
                Debug.Log($"Seed = {this._seed}");
                if (levelAssembler.GenerateLevel(this._seed) is null) {
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
