using System;
using System.Collections;
using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Data;
using ChronosAndCards.Interfaces;

namespace ChronosAndCards.Gameplay.Items
{
    /// <summary>
    /// Gestiona la ventana de tiempo para responder a ataques.
    /// Utiliza corrutinas inyectadas a través de un MonoBehaviour (GameManager) o similar.
    /// </summary>
    public class ReactionWindowManager
    {
        private readonly float _reactionWindowDuration;
        private readonly MonoBehaviour _coroutineRunner;

        public PendingOffensiveEffect ActiveEffect { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="coroutineRunner">MonoBehaviour necesario para ejecutar StartCoroutine.</param>
        /// <param name="duration">Duración en segundos de la ventana de reacción (por defecto 5s).</param>
        public ReactionWindowManager(MonoBehaviour coroutineRunner, float duration = 5f)
        {
            _coroutineRunner = coroutineRunner;
            _reactionWindowDuration = duration;
        }

        /// <summary>
        /// Procesa un efecto ofensivo y abre la ventana de reacción.
        /// </summary>
        public void ProcessOffensiveEffect(IItem item, IPlayer attacker, GameContext context)
        {
            if (ActiveEffect != null)
            {
                Debug.LogWarning("ReactionWindowManager: Ya hay un efecto pendiente en evaluación.");
                return;
            }

            var target = context.TargetPlayer;
            if (target == null)
            {
                Debug.LogWarning("ReactionWindowManager: Efecto ofensivo sin TargetPlayer.");
                item.Effect.Execute(attacker, context); // Aplicar directamente si no hay objetivo (?)
                return;
            }

            ActiveEffect = new PendingOffensiveEffect
            {
                Attacker = attacker,
                Target = target,
                UsedItem = item,
                IsCountered = false,
                ReactionTimeRemaining = _reactionWindowDuration
            };

            // Notificar a la UI
            GameEvents.OnReactionWindowOpened?.Invoke(ActiveEffect, _reactionWindowDuration);

            // Iniciar cuenta regresiva (Hold Then Apply)
            if (_coroutineRunner != null)
            {
                _coroutineRunner.StartCoroutine(ReactionWindowCoroutine(context));
            }
            else
            {
                Debug.LogError("ReactionWindowManager: coroutineRunner es null. Ejecutando efecto inmediatamente.");
                ResolveEffect(context);
            }
        }

        private IEnumerator ReactionWindowCoroutine(GameContext context)
        {
            while (ActiveEffect != null && ActiveEffect.ReactionTimeRemaining > 0)
            {
                ActiveEffect.ReactionTimeRemaining -= Time.deltaTime;
                yield return null;

                // Si fue contrarrestado en este frame, salimos
                if (ActiveEffect != null && ActiveEffect.IsCountered)
                {
                    break;
                }
            }

            if (ActiveEffect != null)
            {
                ResolveEffect(context);
            }
        }

        /// <summary>
        /// Resuelve el efecto al finalizar el temporizador.
        /// Aplica si no fue contrarrestado, o lo anula.
        /// </summary>
        private void ResolveEffect(GameContext context)
        {
            var effectToResolve = ActiveEffect;
            ActiveEffect = null; // Limpiar antes de ejecutar

            if (effectToResolve.IsCountered)
            {
                Debug.Log($"ReactionWindowManager: Efecto ofensivo de '{effectToResolve.UsedItem.Name}' fue contrarrestado. No se aplica.");
            }
            else
            {
                Debug.Log($"ReactionWindowManager: Tiempo agotado. Aplicando efecto '{effectToResolve.UsedItem.Name}'.");
                effectToResolve.UsedItem.Effect.Execute(effectToResolve.Attacker, context);
            }

            GameEvents.OnReactionWindowExpired?.Invoke(effectToResolve);
        }

        /// <summary>
        /// Intenta bloquear el efecto ofensivo actual si hay uno en curso.
        /// </summary>
        public bool TryCounterActiveEffect(IPlayer defender, string counterName)
        {
            if (ActiveEffect == null || ActiveEffect.Target != defender) return false;

            ActiveEffect.IsCountered = true;
            GameEvents.OnCounterActivated?.Invoke(defender, ActiveEffect.Attacker, counterName);
            return true;
        }
    }
}
