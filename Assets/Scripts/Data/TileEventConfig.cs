using UnityEngine;

namespace ChronosAndCards.Data
{
    /// <summary>
    /// Configuración serializable para las casillas de tipo Evento.
    /// Define el tipo de efecto, duración y modificadores de gameplay.
    /// </summary>
    [CreateAssetMenu(fileName = "TileEventConfig", menuName = "Chronos/Tile Event Config")]
    public class TileEventConfig : ScriptableObject
    {
        [Tooltip("Nombre del evento para mostrar en la UI.")]
        public string EventName;

        [TextArea(2, 4)]
        [Tooltip("Descripción del efecto para el jugador.")]
        public string Description;

        [Tooltip("Tipo de modificador que aplica el evento.")]
        public TileEventType EventType;

        [Tooltip("Duración del efecto en turnos. 0 = instantáneo.")]
        [Range(0, 5)]
        public int DurationInTurns;

        [Tooltip("Valor numérico del modificador (si aplica). Ej: +2 para 'avanza 2 extra'.")]
        public int ModifierValue;
    }
}
