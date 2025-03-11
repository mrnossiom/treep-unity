using Treep.State;
using UnityEditor;
using UnityEngine;

namespace Treep.Editor {
    [CustomEditor(typeof(GameStateManager))]
    internal class GameStateManagerEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            GameStateManager gameStateManager = (GameStateManager)this.target;

            GUILayout.Space(15);

            GUILayout.Label($"State kind: {gameStateManager.StateKind}");

            if (Application.isPlaying) {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Level State")) {
                    gameStateManager.TriggerState(GameStateKind.Level);
                }

                GUILayout.EndHorizontal();
            }
            else {
                GUILayout.Label("| Trigger state buttons appear in playing mode");
            }
        }
    }
}
