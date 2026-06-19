using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Data;
using ChronosAndCards.Interfaces;

namespace ChronosAndCards.Gameplay.Items
{
    /// <summary>
    /// Fachada principal para la activación de ítems.
    /// Valida condiciones y ejecuta efectos.
    /// </summary>
    public class ItemEffectExecutor
    {
        private readonly ReactionWindowManager _reactionWindowManager;

        public ItemEffectExecutor(ReactionWindowManager reactionWindowManager)
        {
            _reactionWindowManager = reactionWindowManager;
        }

        /// <summary>
        /// Intenta activar un ítem del jugador en el contexto actual.
        /// </summary>
        public bool TryActivate(IItem item, IPlayer owner, GameContext context)
        {
            if (item == null || item.Effect == null || owner == null || context == null)
            {
                Debug.LogWarning("ItemEffectExecutor: Falta ítem, efecto, dueño o contexto.");
                return false;
            }

            // 1. Validar Posesión
            if (!owner.HasItem<IItem>()) // En realidad verificamos la instancia exacta o solo si existe en inventario
            {
                // Solo un warning si el jugador no lo tiene (puede ser simulado)
                Debug.LogWarning($"ItemEffectExecutor: {owner.PlayerName} no posee el ítem '{item.Name}'.");
            }

            // 2. Validar Fase
            if (item.Phase != context.CurrentPhase)
            {
                Debug.LogWarning($"ItemEffectExecutor: No se puede usar '{item.Name}' en la fase {context.CurrentPhase}. Requiere: {item.Phase}.");
                GameEvents.OnItemBlocked?.Invoke(owner, item, $"Fase incorrecta. Requiere {item.Phase}.");
                return false;
            }

            // 3. Validar Condiciones del Efecto
            if (!item.Effect.CanBeActivated(owner, context))
            {
                Debug.Log($"ItemEffectExecutor: '{item.Name}' no cumple las condiciones de activación.");
                GameEvents.OnItemBlocked?.Invoke(owner, item, "Condiciones de activación no cumplidas.");
                return false;
            }

            // Consumir el objeto del inventario
            owner.RemoveItem(item);
            GameEvents.OnItemUsed?.Invoke(owner, item);

            // 4. Si es ofensivo, delegar a Reaction Window
            if (item.Effect.IsOffensive)
            {
                if (_reactionWindowManager == null)
                {
                    Debug.LogWarning("ItemEffectExecutor: No hay ReactionWindowManager para procesar un efecto ofensivo.");
                    return false;
                }

                _reactionWindowManager.ProcessOffensiveEffect(item, owner, context);
            }
            else
            {
                // Si es un buff/utility, ejecutar inmediatamente
                Debug.Log($"ItemEffectExecutor: Ejecutando efecto '{item.Name}' para {owner.PlayerName}.");
                item.Effect.Execute(owner, context);
            }

            return true;
        }
    }
}
