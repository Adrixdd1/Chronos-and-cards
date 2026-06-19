using UnityEngine;

namespace ChronosAndCards.Interfaces
{
    /// <summary>
    /// Representa un objeto consumible que el jugador puede almacenar y usar.
    /// Cada ítem encapsula su efecto mediante el patrón Strategy.
    /// </summary>
    public interface IItem
    {
        /// <summary>Nombre visible del ítem.</summary>
        string Name { get; }

        /// <summary>Descripción del efecto para la UI.</summary>
        string Description { get; }

        /// <summary>Fase de activación en la que este ítem puede ser usado.</summary>
        ItemActivationPhase Phase { get; }

        /// <summary>Icono para mostrar en el inventario del jugador.</summary>
        Sprite Icon { get; }

        /// <summary>Referencia al efecto que se ejecuta al usar este ítem.</summary>
        IItemEffect Effect { get; }
    }
}
