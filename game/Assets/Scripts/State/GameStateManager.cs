using System;
using Mirror;
using Treep.Level;
using Treep.State.GameStates;
using UnityEngine;
using UnityEngine.Serialization;

namespace Treep.State
{
    public enum GameStateKind
    {
        Lobby,
        Level,
        End,
    }

    public interface IGameState
    {
        // TODO: find another way to pass the manager
        void OnEnter(GameStateManager manager);

        void OnExit();
    }

    public class GameStateManager : NetworkBehaviour
    {
        [SyncVar(hook = nameof(StateKindHook))]
        public GameStateKind stateKind;
        private IGameState _currentState;

        [SerializeField] private LevelAssembler lobbyLevel;
        [SerializeField] private LevelAssembler worldOneLevel;

        public LevelAssembler UglyLobbyAccessorToRemoveLater => lobbyLevel;
        public LevelAssembler UglyLevelAccessorToRemoveLater => worldOneLevel;

        private void Awake()
        {
            stateKind = GameStateKind.Lobby;
        }

        public void StateKindHook(GameStateKind oldStateKind, GameStateKind newStateKind)
        {
            IGameState newState = newStateKind switch
            {
                GameStateKind.Lobby => new GameStateLobby(),
                GameStateKind.Level => new GameStateLevel(5),
                GameStateKind.End => new GameStateEnd(),
                _ => throw new ArgumentOutOfRangeException(nameof(newStateKind), newStateKind, null)
            };

            ChangeState(newState);
        }

        private void ChangeState(IGameState newState)
        {
            if (!isLocalPlayer) return;

            _currentState.OnExit();
            _currentState = newState;
            _currentState.OnEnter(this);
        }
    }
}
