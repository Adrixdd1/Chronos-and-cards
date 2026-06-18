using UnityEngine;
using ChronosAndCards.Data;

namespace ChronosAndCards.Core.States
{
    /// <summary>
    /// Estado encargado de mapear el valor del dado a un nivel de dificultad,
    /// extraer una carta de pregunta y guardarla en el contexto del turno.
    /// </summary>
    public class CardDrawState : IGameState
    {
        private readonly GameManager _gameManager;
        private bool _drawCompleted;

        /// <summary>Constructor que inyecta el GameManager.</summary>
        public CardDrawState(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public void Enter()
        {
            Debug.Log("CardDrawState: Enter");

            int diceValue = _gameManager.GameContext.CurrentDiceValue;
            int difficultyLevel = MapDiceToDifficulty(diceValue);

            CardData card = new CardData(1, "Stub Question", null, null, "Answer", false);
            if (_gameManager.ContentManager != null)
            {
                card = _gameManager.ContentManager.DrawCard(difficultyLevel);
            }
            else
            {
                Debug.LogWarning("CardDrawState: ContentManager no está asignado. Usando carta stub.");
            }

            // Guardar en el contexto
            _gameManager.GameContext.CurrentCard = card;

            // Emitir evento
            GameEvents.OnCardDrawn?.Invoke(card);

            _drawCompleted = true;
        }

        public void Tick()
        {
            if (_drawCompleted)
            {
                _gameManager.TransitionTo(new States.ResolutionState(_gameManager));
            }
        }

        public void Exit()
        {
            Debug.Log("CardDrawState: Exit");
        }

        /// <summary>
        /// Mapea el valor del dado D6 (1–6) a un nivel de dificultad de pregunta.
        /// </summary>
        private int MapDiceToDifficulty(int diceValue)
        {
            // Mapeo simple: el valor del dado es el nivel de dificultad, acotado entre 1 y 6
            return Mathf.Clamp(diceValue, 1, 6);
        }
    }
}
