using Treep.State;
using Treep.State.GameStates;
using UnityEditor;
using UnityEngine;

namespace Treep.Editor
{
    [CustomEditor(typeof(GameStateManager))]
    internal class GameStateManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var gameStateManager = (GameStateManager)target;

            GUILayout.Space(15);

            GUILayout.Label($"State kind: {gameStateManager.stateKind}");

            if (Application.isPlaying)
            {
                if (gameStateManager.netIdentity.isOwned)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Level State"))
                        gameStateManager.stateKind = GameStateKind.Level;

                    GUILayout.EndHorizontal();
                }
                else GUILayout.Label("| Cannot trigger state when not owned");
            }
            else GUILayout.Label("| Trigger state buttons appear in playing mode");
        }
    }
}
