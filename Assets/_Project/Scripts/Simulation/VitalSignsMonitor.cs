using UnityEngine;
using UnityEngine.Events;

namespace PlataformaXR.Simulation
{
    /// <summary>
    /// Controlador principal del Monitor de Signos Vitales.
    /// Coordina ECGSimulator y SpO2Simulator y gestiona el estado del equipo.
    /// </summary>
    public class VitalSignsMonitor : MonoBehaviour
    {
        [Header("Simuladores de señales")]
        public ECGSimulator ecgSimulator;
        public SpO2Simulator spo2Simulator;

        [Header("Estado del monitor")]
        public bool isOn = false;
        public bool isDemoMode = false;
        public bool isAlarmActive = false;

        [Header("Eventos de UI")]
        public UnityEvent<float, float, int> onDisplayUpdated; // (ecgValue, spo2Value, heartRate)
        public UnityEvent<string> onAlarmActivated;
        public UnityEvent onAlarmCleared;

        private float _ecgTime = 0f;

        private void Awake()
        {
            if (spo2Simulator != null)
            {
                spo2Simulator.OnAlarmTriggered += HandleSpO2Alarm;
                spo2Simulator.OnAlarmCleared += HandleAlarmCleared;
            }
            if (ecgSimulator != null)
                ecgSimulator.OnHeartRateChanged += HandleHeartRateChanged;
        }

        private void OnDestroy()
        {
            if (spo2Simulator != null)
            {
                spo2Simulator.OnAlarmTriggered -= HandleSpO2Alarm;
                spo2Simulator.OnAlarmCleared -= HandleAlarmCleared;
            }
            if (ecgSimulator != null)
                ecgSimulator.OnHeartRateChanged -= HandleHeartRateChanged;
        }

        private void Update()
        {
            if (!isOn) return;
            _ecgTime += Time.deltaTime;
            UpdateDisplay();
        }

        /// <summary>Actualiza los valores mostrados en la UI del monitor.</summary>
        public void UpdateDisplay()
        {
            if (ecgSimulator == null || spo2Simulator == null) return;
            float ecgValue  = ecgSimulator.GenerateECGPoint(_ecgTime);
            float spo2Value = spo2Simulator.GetCurrentReading();
            onDisplayUpdated?.Invoke(ecgValue, spo2Value, ecgSimulator.heartRate);
        }

        public void PowerOn()
        {
            isOn = true;
            _ecgTime = 0f;
            Debug.Log("[VitalSignsMonitor] Monitor encendido.");
        }

        public void PowerOff()
        {
            isOn = false;
            isAlarmActive = false;
            Debug.Log("[VitalSignsMonitor] Monitor apagado.");
        }

        /// <summary>
        /// Activa modo demo con valores fisiológicos normales pre-configurados.
        /// </summary>
        public void EnableDemoMode()
        {
            isDemoMode = true;
            if (ecgSimulator != null) { ecgSimulator.heartRate = 72; ecgSimulator.isNormalRhythm = true; ecgSimulator.amplitude = 1.0f; }
            if (spo2Simulator != null) { spo2Simulator.currentSpO2 = 98f; spo2Simulator.isHypoxic = false; }
            if (!isOn) PowerOn();
        }

        public void DisableDemoMode() => isDemoMode = false;

        private void HandleSpO2Alarm(float v) { isAlarmActive = true;  onAlarmActivated?.Invoke($"ALARMA SpO2: {v:F1}% - Nivel critico de oxigenacion"); }
        private void HandleAlarmCleared()     { isAlarmActive = false; onAlarmCleared?.Invoke(); }
        private void HandleHeartRateChanged(int fc) => Debug.Log($"[VitalSignsMonitor] FC: {fc} BPM");
    }
}
