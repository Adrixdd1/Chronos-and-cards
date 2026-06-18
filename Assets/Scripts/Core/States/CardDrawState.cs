using UnityEngine;
using ChronosAndCards.Data;
using ChronosAndCards.Gameplay.Dice;

namespace ChronosAndCards.Core.States
{
    /// <summary>
    /// Estado encargado de mapear el valor del dado a un nivel de dificultad,
    /// extraer una carta de pregunta y guardarla en el contexto del turno.
    /// </summary>
    public class CardDrawState : IGameState
    {
        private readonly GameManager _gameManager;
        private readonly IDifficultyMapper _difficultyMapper;
        private readonly ContentManager _contentManager;
        private bool _drawCompleted;

        /// <summary>Constructor que inyecta GameManager, DifficultyMapper y ContentManager.</summary>
        public CardDrawState(GameManager gameManager, IDifficultyMapper difficultyMapper, ContentManager contentManager)
        {
            _gameManager = gameManager;
            _difficultyMapper = difficultyMapper;
            _contentManager = contentManager;
        }

        public void Enter()
        {
            Debug.Log("CardDrawState: Enter");

            int diceValue = _gameManager.TurnContext.DiceValue;
            
            // Mapear dificultad
            int difficultyLevel = 1;
            if (_difficultyMapper != null)
            {
                difficultyLevel = _difficultyMapper.GetDifficulty(diceValue);
            }
            else
            {
                difficultyLevel = Mathf.Clamp(diceValue, 1, 6);
            }

            // Sabotaje: fuerza dificultad a 6
            if (_gameManager.TurnContext.IsSabotaged)
            {
                difficultyLevel = 6;
            }

            _gameManager.TurnContext.DifficultyLevel = difficultyLevel;

            CardData card;
            if (_contentManager != null)
            {
                card = _contentManager.DrawCard(difficultyLevel);
            }
            else
            {
                Debug.LogWarning("CardDrawState: ContentManager no está asignado. Usando carta stub.");
                card = new CardData(1, "Stub Question", null, null, "Answer", false);
            }

            // Guardar en el TurnContext
            _gameManager.TurnContext.CurrentCard = card;

            // Guardar en el GameContext para flujos/UI retrocompatibles
            _gameManager.GameContext.CurrentCard = card;

            // Emitir evento
            GameEvents.OnCardDrawn?.Invoke(card);

            _drawCompleted = true;
        }

        public void Tick()
        {
            if (_drawCompleted)
            {
                // Transiciona a ResolutionState recibiendo el TurnContext
                _gameManager.TransitionTo(new States.ResolutionState(_gameManager, _gameManager.TurnContext));
            }
        }

        public void Exit()
        {
            Debug.Log("CardDrawState: Exit");
        }
    }
}
