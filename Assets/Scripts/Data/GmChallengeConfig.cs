using UnityEngine;

namespace ChronosAndCards.Data
{
    /// <summary>
    /// Configuración global para los Desafíos del Game Master.
    /// </summary>
    [CreateAssetMenu(fileName = "GmChallengeConfig", menuName = "ChronosAndCards/GmChallengeConfig")]
    public class GmChallengeConfig : ScriptableObject
    {
        [Header("Temporizadores")]
        [SerializeField, Tooltip("Tiempo para responder al Desafío GM")]
        private float _timeLimit = 15f;

        [Header("Controles PC")]
        [SerializeField, Tooltip("Tecla asignada al Jugador 1 para pulsar primero")]
        private KeyCode _player1Key = KeyCode.A;

        [SerializeField, Tooltip("Tecla asignada al Jugador 2 para pulsar primero")]
        private KeyCode _player2Key = KeyCode.L;

        public float TimeLimit => _timeLimit;

        /// <summary>Retorna la tecla configurada para un índice de jugador.</summary>
        public KeyCode GetKeyForPlayer(int playerIndex)
        {
            return playerIndex == 0 ? _player1Key : _player2Key;
        }
    }
}
