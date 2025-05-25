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
        public GameStateKind _stateKind;

        private IGameState _currentState;
        private bool _shouldEnterState;

        [SyncVar(hook = nameof(GameStateManager.OnSeedChanged))]
        public int _seed;

        public int _level = 1;

        public GameStateKind StateKind => this._stateKind;

        [SerializeField] private LevelAssembler lobbyLevel;
        [SerializeField] private LevelAssembler worldOneLevel;
        [SerializeField] private LevelAssembler worldTwoLevel;
        [SerializeField] private GameObject aStarPrefab;

        [SerializeField] public GameObject background1;
        [SerializeField] public GameObject background2;

        [SerializeField] public SpriteRenderer outerLayer;


        public LevelAssembler UglyLobbyAccessorToRemoveLater => this.lobbyLevel;
        public LevelAssembler UglyLevel1AccessorToRemoveLater => this.worldOneLevel;
        public LevelAssembler UglyLevel2AccessorToRemoveLater => this.worldTwoLevel;

        private void Awake() {
            this._stateKind = GameStateKind.Lobby;
            this._currentState = new GameStateLobby();
        }

        [Server]
        public void GenerateAndSetSeed() {
            this._seed = new System.Random().Next();
            Debug.Log($"Generated seed: {this._seed}");
        }

        private void OnSeedChanged(int oldSeed, int newSeed) {
            if (this._stateKind == GameStateKind.Level && newSeed != 0) {
                this.ChangeState(new GameStateLevel(newSeed, this._level));
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
            if (this.isServer && gameState == GameStateKind.Level) {
                this.GenerateAndSetSeed();
            }

            this._level = 1;
            this._stateKind = gameState;
        }

        private void StateKindHook(GameStateKind oldStateKind, GameStateKind newStateKind) {
            if (newStateKind == GameStateKind.Level) {
                if (this._seed == 0) {
                    return;
                }

                this.ChangeState(new GameStateLevel(this._seed, 1));
            }
            else {
                IGameState newState = newStateKind switch {
                    GameStateKind.Lobby => new GameStateLobby(),
                    GameStateKind.End => new GameStateEnd(),
                    _ => throw new ArgumentOutOfRangeException(nameof(newStateKind), newStateKind, null)
                };
                this.ChangeState(newState);
            }
        }

        public void ChangeState(IGameState newState) {
            this._currentState.OnExit();
            this._currentState = newState;
            this._currentState.OnEnter(this);
        }
    }
}
