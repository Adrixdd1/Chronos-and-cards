using UnityEngine;
using ChronosAndCards.Interfaces;
using ChronosAndCards.Data;
using ChronosAndCards.Gameplay.Board;
using ChronosAndCards.Gameplay;

namespace ChronosAndCards.Core.States
{
    /// <summary>
    /// Estado que resuelve el efecto de la casilla en la que el jugador actual aterrizó.
    /// </summary>
    public class TileEffectState : IGameState
    {
        private readonly GameManager _gameManager;
        private readonly BoardManager _boardManager;
        private bool _effectApplied;

        /// <summary>Constructor que inyecta GameManager y BoardManager.</summary>
        public TileEffectState(GameManager gameManager, BoardManager boardManager)
        {
            _gameManager = gameManager;
            _boardManager = boardManager;
        }

        public void Enter()
        {
            Debug.Log("TileEffectState: Enter");

            TurnContext turnContext = _gameManager.TurnContext;
            IPlayer player = turnContext.ActivePlayer;
            if (player == null)
            {
                player = _gameManager.GameContext.CurrentPlayer;
                turnContext.ActivePlayer = player;
            }

            ITile tile = null;
            if (_boardManager != null)
            {
                tile = _boardManager.GetPlayerTile(player);
            }

            if (tile != null)
            {
                // Invocar el efecto de caída
                tile.OnPlayerLanded(player);
                GameEvents.OnTileEffectApplied?.Invoke(player, tile.Type);
            }
            else
            {
                Debug.LogWarning("TileEffectState: BoardManager no asignado o casilla no encontrada. Usando casilla de prueba.");
                // Caída en una casilla de prueba basada en la posición
                TileType resolvedType = (TileType)(player.Position % 5);
                tile = new TestTile(resolvedType);
                tile.OnPlayerLanded(player);
                GameEvents.OnTileEffectApplied?.Invoke(player, resolvedType);
            }

            _effectApplied = true;
        }

        public void Tick()
        {
            if (_effectApplied)
            {
                _gameManager.TransitionTo(new NextPlayerState(_gameManager));
            }
        }

        public void Exit()
        {
            Debug.Log("TileEffectState: Exit");
        }
    }
}
