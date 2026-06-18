using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Data;
using ChronosAndCards.Interfaces;

namespace ChronosAndCards.Gameplay
{
    /// <summary>
    /// Gestiona el consumo de pistas por parte del jugador y su efecto en la penalización de avance del turno.
    /// </summary>
    public class HintSystem
    {
        /// <summary>
        /// Intenta consumir una pista para la carta actual, disminuyendo el saldo y registrando la ayuda en el contexto.
        /// </summary>
        /// <param name="player">El jugador activo.</param>
        /// <param name="card">La carta de pregunta actual.</param>
        /// <param name="turnContext">Contexto del turno actual.</param>
        /// <returns>True si la pista se consumió exitosamente, de lo contrario false.</returns>
        public bool TryUseHint(IPlayer player, CardData card, TurnContext turnContext)
        {
            if (player == null)
            {
                Debug.LogError("HintSystem: El jugador no puede ser nulo.");
                return false;
            }

            if (turnContext == null)
            {
                Debug.LogError("HintSystem: El TurnContext no puede ser nulo.");
                return false;
            }

            // 1. Validar que el jugador tenga pistas
            if (player.HintCount <= 0)
            {
                Debug.LogWarning($"HintSystem: {player.PlayerName} no tiene pistas disponibles.");
                return false;
            }

            // 2. Validar que la carta tenga pista
            if (string.IsNullOrEmpty(card.Hint))
            {
                Debug.LogWarning("HintSystem: La carta actual no tiene una pista disponible.");
                return false;
            }

            // 3. Validar que no se haya usado una pista en este turno
            if (turnContext.UsedHint)
            {
                Debug.LogWarning("HintSystem: Ya se usó una pista en el turno actual.");
                return false;
            }

            // 4. Consumir recurso
            player.AddHint(-1);
            turnContext.UsedHint = true;

            // 5. Lanzar eventos
            GameEvents.OnHintChanged?.Invoke(player, player.HintCount);
            GameEvents.OnHintRevealed?.Invoke(card.Hint);

            Debug.Log($"HintSystem: {player.PlayerName} usó una pista. Pistas restantes: {player.HintCount}");
            return true;
        }

        /// <summary>
        /// Verifica si el jugador cumple con las condiciones para poder consumir una pista.
        /// </summary>
        public bool CanUseHint(IPlayer player, CardData card, TurnContext turnContext)
        {
            if (player == null || turnContext == null) return false;

            return player.HintCount > 0
                && !string.IsNullOrEmpty(card.Hint)
                && !turnContext.UsedHint;
        }
    }
}
