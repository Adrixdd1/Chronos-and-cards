using UnityEngine;
using ChronosAndCards.Interfaces;
using ChronosAndCards.Data;

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

            // Determinar tipo de casilla determinísticamente basada en la posición
            TileType resolvedType = (TileType)(player.Position % 5);
            ITile tile = new TestTile(resolvedType);

            // Invocar el efecto de caída
            tile.OnPlayerLanded(player);

            // Emitir evento
            GameEvents.OnTileEffectApplied?.Invoke(player, resolvedType);

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
