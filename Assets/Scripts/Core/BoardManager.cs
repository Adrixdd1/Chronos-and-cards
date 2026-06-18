using UnityEngine;

namespace ChronosAndCards.Core
{
    /// <summary>
    /// Gestiona el tablero físico/lógico del juego. Stub para compilación de la Épica 1.
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        /// <summary>
        /// Genera el tablero al iniciar la partida.
        /// </summary>
        public void GenerateBoard()
        {
            Debug.Log("BoardManager: Tablero generado (Stub).");
        }

        /// <summary>
        /// Mueve un jugador una cantidad de casillas en el tablero.
        /// </summary>
        /// <param name="player">El jugador a mover.</param>
        /// <param name="tilesToMove">Cantidad de casillas a avanzar.</param>
        public void MovePlayer(IPlayer player, int tilesToMove)
        {
            Debug.Log($"BoardManager: Moviendo jugador {player.PlayerName} {tilesToMove} casillas (Stub).");
            player.MoveForward(tilesToMove);
        }
    }
}
