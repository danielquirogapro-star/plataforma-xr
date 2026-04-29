using System;
using UnityEngine;

namespace PlataformaXR.Simulation
{
    /// <summary>
    /// Simula la señal ECG en tiempo real con forma de onda PQRST aproximada.
    /// </summary>
    public class ECGSimulator : MonoBehaviour
    {
        [Header("Parámetros del ritmo cardíaco")]
        public int heartRate = 72;
        public bool isNormalRhythm = true;
        public float amplitude = 1.0f;

        public event Action<int> OnHeartRateChanged;

        private int _previousHeartRate;
        private const float NoiseFactor = 0.02f;
        private float RRInterval => 60f / Mathf.Max(heartRate, 1);

        private void Awake() => _previousHeartRate = heartRate;

        private void Update()
        {
            if (heartRate != _previousHeartRate)
            {
                _previousHeartRate = heartRate;
                OnHeartRateChanged?.Invoke(heartRate);
            }
        }

        /// <summary>
        /// Genera un punto de la señal ECG para el tiempo dado.
        /// Aproxima matemáticamente las ondas P, Q, R, S y T del ciclo cardíaco.
        /// </summary>
        public float GenerateECGPoint(float time)
        {
            float t = (time % RRInterval) / RRInterval;
            float signal = isNormalRhythm
                ? GenerateNormalPQRST(t)
                : GenerateArrhythmicSignal(t, time);
            signal += UnityEngine.Random.Range(-NoiseFactor, NoiseFactor);
            return signal * amplitude;
        }

        // Genera la onda PQRST normal usando funciones gaussianas
        private float GenerateNormalPQRST(float t)
        {
            float ondaP =  0.15f * Gaussian(t, 0.15f, 0.040f); // despolarización auricular
            float ondaQ = -0.08f * Gaussian(t, 0.38f, 0.015f); // inicio despolarización ventricular
            float ondaR =  1.00f * Gaussian(t, 0.42f, 0.018f); // pico principal
            float ondaS = -0.12f * Gaussian(t, 0.46f, 0.015f); // fin despolarización ventricular
            float ondaT =  0.25f * Gaussian(t, 0.65f, 0.055f); // repolarización ventricular
            return ondaP + ondaQ + ondaR + ondaS + ondaT;
        }

        // Genera señal arrítmica con intervalos y amplitudes irregulares
        private float GenerateArrhythmicSignal(float t, float absoluteTime)
        {
            float offset = 0.08f * Mathf.Sin(absoluteTime * 0.7f);
            float it = t + offset;
            return 0.8f * Gaussian(it, 0.42f, 0.025f)
                 + 0.15f * Gaussian(it, 0.65f, 0.07f);
        }

        // Función gaussiana para modelar la forma suave de las ondas
        private float Gaussian(float x, float mean, float stdDev) =>
            Mathf.Exp(-0.5f * Mathf.Pow((x - mean) / stdDev, 2));

        /// <summary>Activa arritmia para escenario de entrenamiento.</summary>
        public void SimulateArrhythmia()
        {
            isNormalRhythm = false;
            heartRate = UnityEngine.Random.Range(40, 120);
            Debug.Log($"[ECGSimulator] Arritmia activada. FC: {heartRate} BPM");
        }

        /// <summary>Restaura el ritmo sinusal normal.</summary>
        public void RestoreNormalRhythm()
        {
            isNormalRhythm = true;
            heartRate = 72;
            Debug.Log("[ECGSimulator] Ritmo normal restaurado. FC: 72 BPM");
        }
    }
}
