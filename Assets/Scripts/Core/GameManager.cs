using System.Collections.Generic;
using UnityEngine;
using ChronosAndCards.Data;
using ChronosAndCards.Interfaces;
using ChronosAndCards.Gameplay;
using ChronosAndCards.Gameplay.Board;
using ChronosAndCards.Gameplay.Dice;

namespace ChronosAndCards.Core
{
    /// <summary>
    /// Orquestador principal del juego que hostea la FSM, controla las transiciones
    /// de estado y mantiene las referencias serializadas de los managers secundarios.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Managers Referencias")]
        [SerializeField] private BoardManager _boardManager;
        [SerializeField] private ContentManager _contentManager;
        [SerializeField] private DiceRoller _diceRoller;
        [SerializeField] private ContentLoader _contentLoader;

        [Header("Configuraciones del Dado")]
        [SerializeField] private DiceConfig _diceConfig;
        [SerializeField] private DifficultyMapConfig _difficultyMapConfig;
        [SerializeField] private DicePhysics _dicePhysics;

        private DiceLogic _diceLogic;
        private IDifficultyMapper _difficultyMapper;
        private AdvanceCalculator _advanceCalculator;
        private TurnContext _turnContext = new TurnContext();
        private HintSystem _hintSystem;
        private AnswerEvaluator _answerEvaluator;

        private IGameState _currentState;
        private Stack<IGameState> _stateStack = new Stack<IGameState>();

        // Lógica de datos de la sesión
        private List<IPlayer> _players = new List<IPlayer>();
        private int _currentPlayerIndex = 0;
        private GameContext _gameContext = new GameContext();

        /// <summary>Acceso al ContentLoader.</summary>
        public ContentLoader ContentLoader => _contentLoader;

        /// <summary>Acceso al HintSystem.</summary>
        public HintSystem HintSystem => _hintSystem;

        /// <summary>Acceso al AnswerEvaluator.</summary>
        public AnswerEvaluator AnswerEvaluator => _answerEvaluator;

        /// <summary>Acceso al TurnContext compartido.</summary>
        public TurnContext TurnContext => _turnContext;

        /// <summary>Acceso a la lógica del dado.</summary>
        public DiceLogic DiceLogic => _diceLogic;

        /// <summary>Acceso a las físicas del dado.</summary>
        public DicePhysics DicePhysics => _dicePhysics;

        /// <summary>Acceso al mapeador de dificultad.</summary>
        public IDifficultyMapper DifficultyMapper => _difficultyMapper;

        /// <summary>Acceso a la calculadora de avance.</summary>
        public AdvanceCalculator AdvanceCalculator => _advanceCalculator;

        /// <summary>Acceso al BoardManager inyectado.</summary>
        public BoardManager BoardManager => _boardManager;

        /// <summary>Acceso al ContentManager inyectado.</summary>
        public ContentManager ContentManager => _contentManager;

        /// <summary>Acceso al DiceRoller inyectado.</summary>
        public DiceRoller DiceRoller => _diceRoller;

        /// <summary>Lista de jugadores activos en la partida.</summary>
        public List<IPlayer> Players => _players;

        /// <summary>Índice del jugador activo (0-based).</summary>
        public int CurrentPlayerIndex
        {
            get => _currentPlayerIndex;
            set => _currentPlayerIndex = value;
        }

        /// <summary>Contexto dinámico compartido del turno actual.</summary>
        public GameContext GameContext => _gameContext;

        /// <summary>Retorna el estado de la FSM actualmente activo.</summary>
        public IGameState CurrentState => _currentState;

        /// <summary>Retorna el stack actual de estados (interrupciones).</summary>
        public Stack<IGameState> StateStack => _stateStack;

        private void Awake()
        {
            _diceLogic = new DiceLogic();
            _difficultyMapper = new DifficultyMapper(_difficultyMapConfig);
            _advanceCalculator = new AdvanceCalculator();
            _hintSystem = new HintSystem();
            _answerEvaluator = new AnswerEvaluator();
        }

        private void Start()
        {
            // Inicializar en SetupState
            TransitionTo(new States.SetupState(this));
        }

        private void Update()
        {
            // Ejecutar Tick en el estado actual en cada frame
            _currentState?.Tick();
        }

        /// <summary>
        /// Realiza una transición estándar desactivando el estado actual y activando el nuevo.
        /// </summary>
        /// <param name="newState">El nuevo estado al que se transiciona.</param>
        public void TransitionTo(IGameState newState)
        {
            string previousStateName = _currentState != null ? _currentState.GetType().Name : "None";
            string newStateName = newState != null ? newState.GetType().Name : "None";

            Debug.Log($"FSM: {previousStateName} → {newStateName}");

            _currentState?.Exit();
            _currentState = newState;
            _currentState?.Enter();

            GameEvents.OnStateChanged?.Invoke(_currentState);
        }

        /// <summary>
        /// Pausa el estado actual guardándolo en la pila y activa un estado de interrupción (ej. Duelo).
        /// </summary>
        /// <param name="interruptState">El estado de interrupción a activar.</param>
        public void PushState(IGameState interruptState)
        {
            string previousStateName = _currentState != null ? _currentState.GetType().Name : "None";
            string interruptStateName = interruptState != null ? interruptState.GetType().Name : "None";

            Debug.Log($"FSM PUSH: {previousStateName} → {interruptStateName}");

            _stateStack.Push(_currentState);
            _currentState = interruptState;
            _currentState?.Enter();
            
            GameEvents.OnStateChanged?.Invoke(_currentState);
        }

        /// <summary>
        /// Finaliza el estado de interrupción actual y restaura el estado previo desde la pila.
        /// </summary>
        public void PopState()
        {
            if (_stateStack.Count > 0)
            {
                string exitingStateName = _currentState != null ? _currentState.GetType().Name : "None";

                _currentState?.Exit();
                _currentState = _stateStack.Pop();

                string restoredStateName = _currentState != null ? _currentState.GetType().Name : "None";
                Debug.Log($"FSM POP: {exitingStateName} → {restoredStateName}");

                GameEvents.OnStateChanged?.Invoke(_currentState);
            }
            else
            {
                Debug.LogWarning("FSM POP: Intentó hacer pop pero el stack de estados está vacío.");
            }
        }

        private void OnDestroy()
        {
            // Limpieza preventiva de delegados para evitar fugas de memoria
            GameEvents.ClearAll();
        }
    }
}
