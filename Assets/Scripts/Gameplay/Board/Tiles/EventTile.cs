using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Data;

namespace ChronosAndCards.Gameplay.Board
{
    /// <summary>
    /// Casilla que activa un evento fortuito o cambio de reglas temporal, definido por un ScriptableObject.
    /// </summary>
    public class EventTile : BaseTile
    {
        private readonly TileEventConfig _eventConfig;

        public override TileType Type => TileType.Event;

        /// <summary>Acceso a la configuración del evento de esta casilla.</summary>
        public TileEventConfig EventConfig => _eventConfig;

        /// <summary>
        /// Constructor que inyecta la configuración del evento en el índice especificado.
        /// </summary>
        public EventTile(int index, TileEventConfig eventConfig) : base(index)
        {
            _eventConfig = eventConfig;
        }

        public override void OnPlayerLanded(IPlayer player)
        {
            string eventName = _eventConfig != null ? _eventConfig.EventName : "Desconocido";
            string description = _eventConfig != null ? _eventConfig.Description : "Sin descripción";
            int duration = _eventConfig != null ? _eventConfig.DurationInTurns : 0;

            Debug.Log($"EventTile[{Index}]: {player.PlayerName} activó el evento '{eventName}' ({description}). Duración: {duration} turnos.");

            // Si en el futuro existe un sistema de IStatusEffect, se inyectará aquí.
            // Para la Épica 2, la activación se notifica y registra mediante logs y eventos.
            EmitTileEffect(player);
        }
    }
}
