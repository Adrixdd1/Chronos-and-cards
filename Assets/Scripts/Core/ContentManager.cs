using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ChronosAndCards.Data;

namespace ChronosAndCards.Core
{
    /// <summary>
    /// Gestiona las cartas y preguntas del juego, agrupándolas por dificultad y sirviéndolas con fallback y barajado.
    /// </summary>
    public class ContentManager : MonoBehaviour
    {
        private Dictionary<int, Queue<CardData>> _cardsByLevel;
        private Queue<CardData> _gmChallengeCards;

        /// <summary>Se dispara cuando un nivel de dificultad se queda sin cartas.</summary>
        public event Action<int> OnDeckLevelEmpty;

        private void Awake()
        {
            InitializeDecks();
        }

        private void InitializeDecks()
        {
            _cardsByLevel = new Dictionary<int, Queue<CardData>>();
            for (int i = 1; i <= 6; i++)
            {
                _cardsByLevel[i] = new Queue<CardData>();
            }
            _gmChallengeCards = new Queue<CardData>();
        }

        /// <summary>
        /// Inicializa y carga el contenido de las cartas (Stub compatible con Épica 1/2).
        /// </summary>
        public void LoadContent()
        {
            Debug.Log("ContentManager: Cargando contenido stub por defecto.");
            // Carga algunas cartas stubs para iniciar el juego si no se importa archivo
            var stubList = new List<CardData>();
            for (int lvl = 1; lvl <= 6; lvl++)
            {
                stubList.Add(new CardData(lvl, $"Pregunta de prueba del nivel {lvl}?", "Pista de prueba", null, "Correcta", false));
                stubList.Add(new CardData(lvl, $"Pregunta de prueba 2 del nivel {lvl}?", "Pista de prueba 2", null, "Correcta", false));
            }
            stubList.Add(new CardData(5, "Reto GM de prueba?", "Pista de GM", null, "Correcta", true));
            
            LoadCards(stubList, true);
        }

        /// <summary>
        /// Carga una lista de cartas en el mazo, separando normales y de GM, y barajándolas.
        /// </summary>
        /// <param name="cards">Lista de cartas parseadas.</param>
        /// <param name="shuffle">Indica si se deben barajar las cartas al cargar.</param>
        public void LoadCards(List<CardData> cards, bool shuffle = true)
        {
            InitializeDecks();

            if (cards == null) return;

            // Separar normales y GM
            var normalCards = cards.Where(c => !c.IsGmChallenge).ToList();
            var gmCards = cards.Where(c => c.IsGmChallenge).ToList();

            // Agrupar y barajar normales
            for (int level = 1; level <= 6; level++)
            {
                var levelCards = normalCards.Where(c => c.DifficultyLevel == level).ToList();
                if (shuffle)
                {
                    Shuffle(levelCards);
                }
                _cardsByLevel[level] = new Queue<CardData>(levelCards);
            }

            // Barajar y encolar cartas de GM
            if (shuffle)
            {
                Shuffle(gmCards);
            }
            _gmChallengeCards = new Queue<CardData>(gmCards);

            Debug.Log($"ContentManager: Mazo cargado. Total de cartas restantes: {TotalRemainingCards}");
        }

        /// <summary>
        /// Extrae la siguiente carta del nivel solicitado.
        /// </summary>
        /// <param name="difficultyLevel">Nivel de dificultad (1–6).</param>
        /// <returns>Estructura CardData con la información de la pregunta.</returns>
        public CardData DrawCard(int difficultyLevel)
        {
            CardData card = DrawWithFallback(difficultyLevel);
            
            // Si la cola del nivel solicitado se vació tras este draw, emite evento
            if (difficultyLevel >= 1 && difficultyLevel <= 6)
            {
                if (GetRemainingCards(difficultyLevel) == 0)
                {
                    OnDeckLevelEmpty?.Invoke(difficultyLevel);
                    GameEvents.OnDeckLevelEmpty?.Invoke(difficultyLevel);
                }
            }

            return card;
        }

        /// <summary>
        /// Extrae una carta del pool del GM.
        /// </summary>
        public CardData DrawGmChallenge()
        {
            if (_gmChallengeCards != null && _gmChallengeCards.Count > 0)
            {
                return _gmChallengeCards.Dequeue();
            }

            Debug.LogWarning("ContentManager: Mazo GM vacío. Haciendo fallback a cartas normales de nivel 4-6.");
            for (int lvl = 6; lvl >= 4; lvl--)
            {
                if (HasCardsAvailable(lvl))
                {
                    return _cardsByLevel[lvl].Dequeue();
                }
            }

            // Fallback total por proximidad al nivel 5
            return DrawWithFallback(5);
        }

        /// <summary>
        /// Retorna cuántas cartas quedan disponibles en un nivel.
        /// </summary>
        public int GetRemainingCards(int level)
        {
            if (_cardsByLevel != null && _cardsByLevel.TryGetValue(level, out var q))
            {
                return q.Count;
            }
            return 0;
        }

        /// <summary>
        /// Retorna si hay cartas disponibles en el nivel.
        /// </summary>
        public bool HasCardsAvailable(int level)
        {
            return GetRemainingCards(level) > 0;
        }

        /// <summary>
        /// Retorna el total de cartas restantes en el mazo normal y GM.
        /// </summary>
        public int TotalRemainingCards
        {
            get
            {
                int count = 0;
                if (_cardsByLevel != null)
                {
                    foreach (var kvp in _cardsByLevel)
                    {
                        count += kvp.Value.Count;
                    }
                }
                if (_gmChallengeCards != null)
                {
                    count += _gmChallengeCards.Count;
                }
                return count;
            }
        }

        private CardData DrawWithFallback(int requestedLevel)
        {
            // Intentar nivel solicitado
            if (HasCardsAvailable(requestedLevel))
            {
                return _cardsByLevel[requestedLevel].Dequeue();
            }

            // Búsqueda por proximidad (priorizando el más fácil N-offset)
            for (int offset = 1; offset <= 5; offset++)
            {
                int lower = requestedLevel - offset;
                int upper = requestedLevel + offset;

                if (lower >= 1 && HasCardsAvailable(lower))
                {
                    Debug.LogWarning($"Fallback: nivel {requestedLevel} agotado, usando nivel {lower}");
                    return _cardsByLevel[lower].Dequeue();
                }
                if (upper <= 6 && HasCardsAvailable(upper))
                {
                    Debug.LogWarning($"Fallback: nivel {requestedLevel} agotado, usando nivel {upper}");
                    return _cardsByLevel[upper].Dequeue();
                }
            }

            // Todos los niveles agotados
            Debug.LogError("ContentManager: todas las cartas se han agotado.");
            return CreateEmptyCard();
        }

        private void Shuffle<T>(List<T> list)
        {
            var rng = new System.Random();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        /// <summary>
        /// Crea una carta vacía de emergencia en caso de agotar todas las preguntas.
        /// </summary>
        private CardData CreateEmptyCard()
        {
            return new CardData(
                1,
                "No hay más preguntas disponibles. El GM debe cargar más contenido.",
                null,
                null,
                "N/A",
                false
            );
        }
    }
}
