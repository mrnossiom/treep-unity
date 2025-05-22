using System;
using Mirror;
using Treep.Level;
using Treep.State.GameStates;
using UnityEngine;

namespace Treep.State {
    public enum GameStateKind {
        Lobby,
        Level,
        End
    }

    public interface IGameState {
        // TODO: find another way to pass the manager
        void OnEnter(GameStateManager manager);

        void OnExit();
    }

    public class GameStateManager : NetworkBehaviour {
        [SyncVar(hook = nameof(GameStateManager.StateKindHook))]
        private GameStateKind _stateKind;

        private IGameState _currentState;
        private bool _shouldEnterState;

        public GameStateKind StateKind => this._stateKind;

        [SerializeField] private LevelAssembler lobbyLevel;
        [SerializeField] private LevelAssembler worldOneLevel;

        public LevelAssembler UglyLobbyAccessorToRemoveLater => this.lobbyLevel;
        public LevelAssembler UglyLevelAccessorToRemoveLater => this.worldOneLevel;

        private void Awake() {
            this._stateKind = GameStateKind.Lobby;
            this._currentState = new GameStateLobby();
        }

        public override void OnStartClient() {
            base.OnStartClient();
            this._shouldEnterState = true;
        }

        public void Update() {
            if (this._shouldEnterState && NetworkClient.connection.identity != null) {
                this._currentState.OnEnter(this);
                this._shouldEnterState = false;
            }
        }
        
        public void TriggerState(GameStateKind gameState) {
            this._stateKind = gameState;
        }

        private void StateKindHook(GameStateKind oldStateKind, GameStateKind newStateKind) {
            IGameState newState = newStateKind switch {
                GameStateKind.Lobby => new GameStateLobby(),
                GameStateKind.Level => new GameStateLevel(5),
                GameStateKind.End => new GameStateEnd(),
                _ => throw new ArgumentOutOfRangeException(nameof(newStateKind), newStateKind, null)
            };

            this.ChangeState(newState);
        }

        private void ChangeState(IGameState newState) {
            this._currentState.OnExit();
            this._currentState = newState;
            this._currentState.OnEnter(this);
        }
    }
}
