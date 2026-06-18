using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChronosAndCards.Data;

namespace ChronosAndCards.Gameplay.Dice
{
    /// <summary>
    /// Componente encargado de la animación física y del alineamiento visual final del dado.
    /// </summary>
    public class DicePhysics : MonoBehaviour
    {
        [SerializeField] private DiceConfig _config;
        [SerializeField] private Rigidbody _rigidbody;

        /// <summary>Evento disparado al completarse la simulación visual del dado.</summary>
        public event Action OnDiceAnimationComplete;

        /// <summary>Timeout de seguridad para la animación de lanzamiento.</summary>
        public float AnimationTimeout => _config != null ? _config.AnimationTimeout : 6f;

        private static readonly Dictionary<int, Quaternion> _faceRotations = new()
        {
            { 1, Quaternion.Euler(0, 0, 0) },       // Cara 1 arriba
            { 2, Quaternion.Euler(-90, 0, 0) },     // Cara 2 arriba
            { 3, Quaternion.Euler(0, 0, 90) },      // Cara 3 arriba
            { 4, Quaternion.Euler(0, 0, -90) },     // Cara 4 arriba
            { 5, Quaternion.Euler(90, 0, 0) },      // Cara 5 arriba
            { 6, Quaternion.Euler(180, 0, 0) },     // Cara 6 arriba
        };

        private Coroutine _animationCoroutine;
        private Vector3 _startLocalPos;

        private void Awake()
        {
            if (_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody>();
            
            _startLocalPos = transform.position;
        }

        /// <summary>
        /// Inicia la simulación visual del dado, aplicando fuerzas y orientándolo al resultado lógico.
        /// </summary>
        /// <param name="targetValue">La cara lógica (1–6) que debe quedar boca arriba.</param>
        public void AnimateToResult(int targetValue)
        {
            Debug.Log($"DicePhysics: AnimateToResult invocado para cara {targetValue}.");
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }

            _animationCoroutine = StartCoroutine(DiceAnimationSequence(targetValue));
        }

        private IEnumerator DiceAnimationSequence(int targetValue)
        {
            // 1. Preparar e iniciar lanzamiento
            transform.position = new Vector3(_startLocalPos.x, _config.LaunchHeight, _startLocalPos.z);
            _rigidbody.isKinematic = false;
            
            // Fuerza hacia arriba más una pequeña desviación aleatoria horizontal
            Vector3 forceVec = Vector3.up * _config.LaunchForce + 
                               new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f)) * 0.2f * _config.LaunchForce;
            
            Vector3 torqueVec = new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f)
            ).normalized * _config.TorqueForce;

            _rigidbody.AddForce(forceVec, ForceMode.Impulse);
            _rigidbody.AddTorque(torqueVec, ForceMode.Impulse);

            float elapsedTime = 0f;
            bool reachedSettle = false;

            // 2. Fase de simulación física activa
            while (elapsedTime < _config.MaxRollDuration)
            {
                yield return null;
                elapsedTime += Time.deltaTime;

                // Si se detiene físicamente (velocidad angular cae bajo umbral)
                if (_rigidbody.angularVelocity.magnitude < _config.StopThreshold && 
                    _rigidbody.velocity.magnitude < _config.StopThreshold)
                {
                    reachedSettle = true;
                    break;
                }
            }

            if (!reachedSettle)
            {
                Debug.LogWarning("DicePhysics: MaxRollDuration alcanzada. Forzando detención de físicas.");
            }

            // 3. Fase de settle (acomodo de rotación)
            _rigidbody.isKinematic = true;

            if (!_faceRotations.TryGetValue(targetValue, out Quaternion targetRotation))
            {
                Debug.LogError($"DicePhysics: Cara no soportada {targetValue}. Usando rotación por defecto.");
                targetRotation = Quaternion.identity;
            }

            float settleElapsedTime = 0f;
            float safetyTimeout = _config.AnimationTimeout - elapsedTime;
            if (safetyTimeout < 1f) safetyTimeout = 2f; // Dar margen mínimo

            while (settleElapsedTime < safetyTimeout)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _config.SettleSpeed);
                
                if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
                {
                    transform.rotation = targetRotation;
                    break;
                }

                yield return null;
                settleElapsedTime += Time.deltaTime;
            }

            // Si se superó el timeout de seguridad, forzar rotación final inmediatamente
            if (Quaternion.Angle(transform.rotation, targetRotation) >= 1f)
            {
                Debug.LogWarning("DicePhysics: Timeout de animación alcanzado. Acomodando rotación final instantáneamente.");
                transform.rotation = targetRotation;
            }

            Debug.Log($"DicePhysics: Animación completada para cara {targetValue}.");
            _animationCoroutine = null;
            OnDiceAnimationComplete?.Invoke();
        }
    }
}
