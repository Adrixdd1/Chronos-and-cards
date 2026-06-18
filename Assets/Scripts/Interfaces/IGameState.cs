namespace ChronosAndCards.Core
{
    /// <summary>
    /// Contrato base que define el ciclo de vida de los estados del juego
    /// dentro de la Máquina de Estados Finitos (FSM).
    /// </summary>
    public interface IGameState
    {
        /// <summary>
        /// Se invoca una vez al activar e ingresar a este estado.
        /// </summary>
        void Enter();

        /// <summary>
        /// Se invoca en cada frame (Update) mientras el estado está activo.
        /// </summary>
        void Tick();

        /// <summary>
        /// Se invoca una vez al desactivar y salir de este estado.
        /// </summary>
        void Exit();
    }
}
