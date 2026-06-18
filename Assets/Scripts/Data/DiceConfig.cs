using UnityEngine;

namespace ChronosAndCards.Data
{
    /// <summary>
    /// Configuración de físicas y animación del dado.
    /// </summary>
    [CreateAssetMenu(fileName = "DiceConfig_Default", menuName = "Chronos/Dice Config")]
    public class DiceConfig : ScriptableObject
    {
        [Header("Físicas del Lanzamiento")]
        [Tooltip("Fuerza aplicada al dado al lanzar (impulso vertical).")]
        [Range(1f, 20f)]
        public float LaunchForce = 8f;

        [Tooltip("Torque aplicado al dado al lanzar (rotación aleatoria).")]
        [Range(1f, 30f)]
        public float TorqueForce = 15f;

        [Tooltip("Altura desde la que se lanza el dado.")]
        [Range(1f, 10f)]
        public float LaunchHeight = 5f;

        [Header("Animación")]
        [Tooltip("Tiempo máximo (segundos) que el dado puede rodar antes de forzar el resultado.")]
        [Range(1f, 8f)]
        public float MaxRollDuration = 4f;

        [Tooltip("Velocidad del Slerp para orientar el dado al resultado final.")]
        [Range(0.5f, 10f)]
        public float SettleSpeed = 3f;

        [Tooltip("Umbral de velocidad angular para considerar que el dado se detuvo.")]
        [Range(0.01f, 1f)]
        public float StopThreshold = 0.1f;

        [Header("Timeout de Seguridad")]
        [Tooltip("Tiempo máximo (segundos) antes de forzar la finalización de la animación.")]
        [Range(3f, 15f)]
        public float AnimationTimeout = 6f;
    }
}
