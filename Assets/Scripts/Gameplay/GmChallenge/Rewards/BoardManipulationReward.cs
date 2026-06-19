using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Data;
using ChronosAndCards.Interfaces;

namespace ChronosAndCards.Gameplay.GmChallenge.Rewards
{
    /// <summary>
    /// Recompensa "Manipulación del Tablero" del Desafío del GM.
    /// El ganador elige dos casillas adyacentes del tablero y las intercambia.
    /// Es una recompensa interactiva que requiere selección vía UI.
    /// </summary>
    public class BoardManipulationReward : IGmReward
    {
        public string Name => "Manipulación del Tablero";
        public string Description => "Elige dos casillas adyacentes e intercámbielas.";
        public GmRewardType RewardType => GmRewardType.BoardManipulation;
        public bool RequiresInteraction => true;

        /// <summary>
        /// Inicia el flujo interactivo de selección de casillas.
        /// La UI gestionará la selección y notificará la resolución vía eventos.
        /// </summary>
        public void Apply(IPlayer winner, GameContext context)
        {
            if (context.BoardManager == null)
            {
                Debug.LogWarning("BoardManipulationReward: BoardManager es null. Recompensa cancelada.");
                return;
            }

            // Obtener casillas intercambiables (excluyendo Inicio y Meta)
            var swappableTiles = context.BoardManager.GetSwappableTiles();

            if (swappableTiles.Count < 2)
            {
                Debug.LogWarning("BoardManipulationReward: No hay suficientes casillas intercambiables. Recompensa cancelada.");
                return;
            }

            // Solicitar selección a la UI
            GameEvents.OnBoardManipulationStarted?.Invoke(winner, swappableTiles);

            // Suscribirse al resultado
            GameEvents.OnBoardManipulationCompleted += OnManipulationCompleted;

            Debug.Log($"BoardManipulationReward: {winner.PlayerName} puede seleccionar dos casillas adyacentes para intercambiar.");
        }

        /// <summary>Callback cuando la UI confirma la selección de casillas.</summary>
        private void OnManipulationCompleted(ITile tileA, ITile tileB)
        {
            GameEvents.OnBoardManipulationCompleted -= OnManipulationCompleted;
            Debug.Log($"BoardManipulationReward: Casillas intercambiadas: '{tileA.Type}' ↔ '{tileB.Type}'.");
        }
    }
}
