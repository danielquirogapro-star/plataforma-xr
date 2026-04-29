using System;
using System.Collections;
using UnityEngine;

namespace PlataformaXR.Simulation
{
    /// <summary>
    /// Simula la saturación de oxígeno (SpO2) con variaciones fisiológicas y estados de hipoxia.
    /// </summary>
    public class SpO2Simulator : MonoBehaviour
    {
        [Header("Estado actual de SpO2")]
        [Range(85f, 100f)] public float currentSpO2 = 98f;
        public bool isHypoxic = false;

        private const float AlarmThreshold = 90f;
        private const float PhysiologicalVariation = 0.5f;
        private bool _alarmActive = false;
        private Coroutine _hypoxiaCoroutine;

        // Evento que se dispara cuando SpO2 cae por debajo del umbral de alarma
        public event Action<float> OnAlarmTriggered;
        // Evento que se dispara cuando SpO2 vuelve a nivel seguro
        public event Action OnAlarmCleared;

        private void Update() => CheckAlarmStatus();

        /// <summary>
        /// Retorna lectura actual con variación aleatoria realista (+/- 0.5%).
        /// </summary>
        public float GetCurrentReading() =>
            Mathf.Clamp(currentSpO2 + UnityEngine.Random.Range(-PhysiologicalVariation, PhysiologicalVariation), 0f, 100f);

        /// <summary>
        /// Baja gradualmente el SpO2 hasta el valor objetivo (0.5% por segundo).
        /// </summary>
        public void SimulateHypoxia(float targetValue)
        {
            targetValue = Mathf.Clamp(targetValue, 85f, 99f);
            if (_hypoxiaCoroutine != null) StopCoroutine(_hypoxiaCoroutine);
            _hypoxiaCoroutine = StartCoroutine(GradualHypoxia(targetValue));
            Debug.Log($"[SpO2Simulator] Hipoxia iniciada -> objetivo: {targetValue}%");
        }

        /// <summary>
        /// Restaura el SpO2 a 98% de forma gradual.
        /// </summary>
        public void RestoreNormalOxygenation()
        {
            if (_hypoxiaCoroutine != null) StopCoroutine(_hypoxiaCoroutine);
            _hypoxiaCoroutine = StartCoroutine(GradualRecovery(98f));
            isHypoxic = false;
        }

        private IEnumerator GradualHypoxia(float target)
        {
            isHypoxic = true;
            while (currentSpO2 > target)
            {
                currentSpO2 = Mathf.Max(currentSpO2 - 0.5f, target);
                yield return new WaitForSeconds(1f);
            }
        }

        private IEnumerator GradualRecovery(float target)
        {
            while (currentSpO2 < target)
            {
                currentSpO2 = Mathf.Min(currentSpO2 + 1.0f, target);
                yield return new WaitForSeconds(0.5f);
            }
            isHypoxic = false;
        }

        private void CheckAlarmStatus()
        {
            if (currentSpO2 < AlarmThreshold && !_alarmActive)
            {
                _alarmActive = true;
                OnAlarmTriggered?.Invoke(currentSpO2);
                Debug.LogWarning($"[SpO2Simulator] ALARMA: SpO2 critico = {currentSpO2:F1}%");
            }
            else if (currentSpO2 >= AlarmThreshold && _alarmActive)
            {
                _alarmActive = false;
                OnAlarmCleared?.Invoke();
            }
        }
    }
}
