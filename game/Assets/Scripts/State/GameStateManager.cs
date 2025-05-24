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
        
        [SyncVar(hook = nameof(OnSeedChanged))]
        private int _seed;

        public GameStateKind StateKind => this._stateKind;

        [SerializeField] private LevelAssembler lobbyLevel;
        [SerializeField] private LevelAssembler worldOneLevel;
        [SerializeField] private GameObject aStarPrefab;

        public LevelAssembler UglyLobbyAccessorToRemoveLater => this.lobbyLevel;
        public LevelAssembler UglyLevelAccessorToRemoveLater => this.worldOneLevel;
        public GameObject AStarPrefab => this.aStarPrefab;

        private void Awake() {
            this._stateKind = GameStateKind.Lobby;
            this._currentState = new GameStateLobby();
        }
        
        [Server]
        public void GenerateAndSetSeed() {
            _seed = new System.Random().Next();
            Debug.Log($"Generated seed: {_seed}");
        }

        private void OnSeedChanged(int oldSeed, int newSeed) {
            if (_stateKind == GameStateKind.Level && newSeed != 0) {
                ChangeState(new GameStateLevel(newSeed));
            }
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
            if (isServer && gameState == GameStateKind.Level) {
                GenerateAndSetSeed();
            }
            this._stateKind = gameState;
        }

        private void StateKindHook(GameStateKind oldStateKind, GameStateKind newStateKind) {
            if (newStateKind == GameStateKind.Level) {
                if (_seed == 0) {
                    return;
                }
                ChangeState(new GameStateLevel(_seed));
            }
            else {
                IGameState newState = newStateKind switch {
                    GameStateKind.Lobby => new GameStateLobby(),
                    GameStateKind.End => new GameStateEnd(),
                    _ => throw new ArgumentOutOfRangeException(nameof(newStateKind), newStateKind, null)
                };
                ChangeState(newState);
            }
        }

        private void ChangeState(IGameState newState) {
            this._currentState.OnExit();
            this._currentState = newState;
            this._currentState.OnEnter(this);
        }
    }
}
