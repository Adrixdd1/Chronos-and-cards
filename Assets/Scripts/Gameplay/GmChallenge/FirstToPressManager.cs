using System;
using System.Collections.Generic;
using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Data;

namespace ChronosAndCards.Gameplay.GmChallenge
{
    /// <summary>
    /// Gestiona la mecánica "El primero en pulsar" para decidir quién
    /// contesta el Desafío del GM.
    /// Soporta tanto polling de teclado (PC) como métodos directos desde la UI (Mobile).
    /// </summary>
    public class FirstToPressManager : MonoBehaviour
    {
        private bool _isListening = false;
        private List<IPlayer> _eligiblePlayers;
        private GmChallengeConfig _config;

        /// <summary>Se dispara cuando un jugador presiona primero (ya sea tecla o botón).</summary>
        public event Action<IPlayer> OnFirstPlayerPressed;

        /// <summary>Inicia la escucha de inputs.</summary>
        public void StartListening(List<IPlayer> players, GmChallengeConfig config)
        {
            _eligiblePlayers = players;
            _config = config;
            _isListening = true;

            Debug.Log("FirstToPressManager: Escuchando inputs...");
        }

        /// <summary>Detiene la escucha de inputs.</summary>
        public void StopListening()
        {
            _isListening = false;
            _eligiblePlayers = null;
        }

        /// <summary>
        /// Puede ser llamado por los botones de la UI (para soporte Multi-Touch Mobile).
        /// Resuelve concurrencia delegando al primer evento que llegue.
        /// </summary>
        public void NotifyPlayerPressed(IPlayer player)
        {
            if (!_isListening) return;

            Debug.Log($"FirstToPressManager: {player.PlayerName} presionó desde la UI.");
            StopListening();
            OnFirstPlayerPressed?.Invoke(player);
        }

        private void Update()
        {
            if (!_isListening || _eligiblePlayers == null || _config == null) return;

            IPlayer pressedPlayer = null;

            foreach (var player in _eligiblePlayers)
            {
                KeyCode key = _config.GetKeyForPlayer(player.PlayerIndex);
                if (Input.GetKeyDown(key))
                {
                    if (pressedPlayer == null)
                    {
                        pressedPlayer = player;
                    }
                    else
                    {
                        // Empate en el mismo frame (presiones simultáneas en teclado)
                        Debug.LogWarning("FirstToPressManager: Empate detectado en el mismo frame. Resolviendo por azar.");
                        if (UnityEngine.Random.value > 0.5f)
                        {
                            pressedPlayer = player;
                        }
                    }
                }
            }

            if (pressedPlayer != null)
            {
                Debug.Log($"FirstToPressManager: {pressedPlayer.PlayerName} presionó desde Teclado.");
                StopListening();
                OnFirstPlayerPressed?.Invoke(pressedPlayer);
            }
        }
    }
}
