using UnityEngine;
using ChronosAndCards.Interfaces;
using ChronosAndCards.Data;
using ChronosAndCards.Gameplay.Board;

namespace ChronosAndCards.Core.States
{
    /// <summary>
    /// Estado que resuelve el efecto de la casilla en la que el jugador actual aterrizó.
    /// </summary>
    public class TileEffectState : IGameState
    {
        private readonly GameManager _gameManager;
        private bool _effectApplied;

        /// <summary>Constructor que inyecta el GameManager.</summary>
        public TileEffectState(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public void Enter()
        {
            Debug.Log("TileEffectState: Enter");

            IPlayer player = _gameManager.GameContext.CurrentPlayer;
            ITile tile = null;

            if (_gameManager.BoardManager != null)
            {
                tile = _gameManager.BoardManager.GetPlayerTile(player);
            }

            if (tile != null)
            {
                // Invocar el efecto de caída
                tile.OnPlayerLanded(player);
            }
            else
            {
                Debug.LogWarning("TileEffectState: BoardManager no asignado o casilla no encontrada. Usando casilla de prueba.");
                // Caída en una casilla de prueba basada en la posición
                TileType resolvedType = (TileType)(player.Position % 5);
                tile = new TestTile(resolvedType);
                tile.OnPlayerLanded(player);
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
