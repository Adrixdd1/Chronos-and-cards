using System;

namespace ChronosAndCards.Interfaces
{
    /// <summary>
    /// Contrato que define el lanzador de dados físico o virtual, y expone
    /// el evento con el resultado final obtenido.
    /// </summary>
    public interface IDiceRoller
    {
        /// <summary>Evento disparado cuando el dado termina de rodar y entrega un resultado.</summary>
        event Action<int> OnDiceResult;

        /// <summary>
        /// Inicia la acción física o lógica de lanzar el dado.
        /// </summary>
        void Roll();
    }
}
