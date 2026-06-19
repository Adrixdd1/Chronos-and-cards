using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ChronosAndCards.Interfaces;
using ChronosAndCards.Core;

namespace ChronosAndCards.Gameplay.StatusEffects
{
    /// <summary>
    /// Gestiona los efectos de estado temporales de todos los jugadores.
    /// Aplica, actualiza y remueve efectos basándose en rondas, turnos y uso.
    /// </summary>
    public class StatusEffectManager
    {
        /// <summary>Se dispara cuando un efecto de estado se aplica.</summary>
        public event Action<IPlayer, IStatusEffect> OnEffectApplied;

        /// <summary>Se dispara cuando un efecto de estado expira.</summary>
        public event Action<IPlayer, IStatusEffect> OnEffectExpired;

        /// <summary>
        /// Añade un efecto de estado a un jugador.
        /// </summary>
        public void AddEffect(IPlayer player, IStatusEffect effect)
        {
            player.AddStatusEffect(effect);
            OnEffectApplied?.Invoke(player, effect);
            GameEvents.OnStatusEffectApplied?.Invoke(player, effect);

            Debug.Log($"StatusEffectManager: '{effect.Name}' aplicado a {player.PlayerName}.");
        }

        /// <summary>
        /// Aplica los efectos activos del jugador al turno actual.
        /// Llamar al inicio de cada turno, antes de la fase de respuesta.
        /// </summary>
        public void ApplyEffectsToTurn(IPlayer player, TurnContext turnContext)
        {
            foreach (var effect in player.ActiveEffects.ToList())
            {
                if (effect.IsActive && effect.ShouldActivate(turnContext))
                {
                    effect.ApplyToTurn(turnContext);
                    Debug.Log($"StatusEffectManager: '{effect.Name}' activado para {player.PlayerName} en este turno.");
                }
            }
        }

        /// <summary>
        /// Notifica a todos los efectos que la ronda ha cambiado.
        /// Remueve efectos expirados.
        /// </summary>
        public void OnRoundChanged(List<IPlayer> players, int currentRound)
        {
            foreach (var player in players)
            {
                var expiredEffects = new List<IStatusEffect>();

                foreach (var effect in player.ActiveEffects)
                {
                    // Notificar cambio de ronda a efectos basados en rondas
                    if (effect is ImmunityShieldStatusEffect shield)
                    {
                        shield.OnRoundChanged(currentRound);
                    }

                    if (effect.IsExpired)
                    {
                        expiredEffects.Add(effect);
                    }
                }

                // Remover efectos expirados
                foreach (var expired in expiredEffects)
                {
                    player.RemoveStatusEffect(expired);
                    OnEffectExpired?.Invoke(player, expired);
                    GameEvents.OnStatusEffectExpired?.Invoke(player, expired);

                    Debug.Log($"StatusEffectManager: '{expired.Name}' expiró para {player.PlayerName}.");
                }
            }
        }

        /// <summary>
        /// Limpia los efectos de un solo uso que ya fueron consumidos.
        /// Llamar al final de cada turno.
        /// </summary>
        public void CleanupExpiredEffects(IPlayer player)
        {
            var expired = player.ActiveEffects.Where(e => e.IsExpired).ToList();
            foreach (var effect in expired)
            {
                player.RemoveStatusEffect(effect);
                OnEffectExpired?.Invoke(player, effect);
                GameEvents.OnStatusEffectExpired?.Invoke(player, effect);
            }
        }
    }
}
