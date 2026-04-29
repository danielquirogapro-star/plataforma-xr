using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlataformaXR.Data
{
    /// <summary>
    /// Gestiona el progreso del estudiante con PlayerPrefs (sin Firebase por ahora).
    /// </summary>
    public class TrainingProgressManager : MonoBehaviour
    {
        private const string HistoryKey       = "XR_TrainingHistory";
        private const int    MaxSessionsStored = 50;
        private const int    BaseScore         = 100;
        private const int    PenaltyPerError   = 10;

        [Header("Sesion actual")]
        public TrainingSession currentSession;
        public bool sessionInProgress = false;

        /// <summary>Inicia una nueva sesion con la fecha y hora actuales.</summary>
        public void StartSession(string trainingType = "General")
        {
            currentSession = new TrainingSession
            {
                fecha             = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                tipoEntrenamiento = trainingType,
                erroresCometidos  = new List<string>()
            };
            sessionInProgress = true;
            Debug.Log($"[TrainingProgressManager] Sesion iniciada: {trainingType}");
        }

        /// <summary>Registra un error cometido durante la sesion activa.</summary>
        public void RegisterError(string descripcionError)
        {
            if (!sessionInProgress || currentSession == null) return;
            currentSession.erroresCometidos.Add(descripcionError);
        }

        /// <summary>Finaliza la sesion, calcula el puntaje y guarda en historial.</summary>
        public void EndSession(float duracionSegundos)
        {
            if (!sessionInProgress || currentSession == null) return;
            currentSession.duracionSegundos = duracionSegundos;
            currentSession.puntaje          = CalculateScore(currentSession);
            sessionInProgress               = false;
            SaveSession();
            Debug.Log($"[TrainingProgressManager] Sesion finalizada. Puntaje: {currentSession.puntaje}/100");
        }

        /// <summary>
        /// Guarda la sesion en PlayerPrefs.
        /// Si se supera el limite de sesiones almacenadas, descarta la mas antigua.
        /// </summary>
        public void SaveSession()
        {
            if (currentSession == null) return;
            var history = LoadHistory();
            history.Add(currentSession);
            if (history.Count > MaxSessionsStored) history.RemoveAt(0);
            PlayerPrefs.SetString(HistoryKey, JsonUtility.ToJson(new TrainingSessionList { sessions = history }));
            PlayerPrefs.Save();
        }

        /// <summary>Carga el historial completo desde PlayerPrefs.</summary>
        public List<TrainingSession> LoadHistory()
        {
            string json = PlayerPrefs.GetString(HistoryKey, "");
            if (string.IsNullOrEmpty(json)) return new List<TrainingSession>();
            var lista = JsonUtility.FromJson<TrainingSessionList>(json);
            return lista?.sessions ?? new List<TrainingSession>();
        }

        public float GetAverageScore()
        {
            var h = LoadHistory();
            if (h.Count == 0) return 0f;
            float t = 0f;
            foreach (var s in h) t += s.puntaje;
            return t / h.Count;
        }

        /// <summary>
        /// Calcula el puntaje: 100 - (errores x 10) + 5 bonus si termina en menos de 10 min.
        /// </summary>
        public int CalculateScore(TrainingSession session)
        {
            if (session == null) return 0;
            int score = BaseScore - ((session.erroresCometidos?.Count ?? 0) * PenaltyPerError);
            if (session.duracionSegundos > 0 && session.duracionSegundos < 600f) score += 5;
            return Mathf.Clamp(score, 0, 100);
        }

        public void ClearAllProgress() { PlayerPrefs.DeleteKey(HistoryKey); PlayerPrefs.Save(); }
    }

    /// <summary>
    /// Representa una sesion de entrenamiento completa. Serializable para JsonUtility.
    /// </summary>
    [Serializable]
    public class TrainingSession
    {
        public string fecha;
        public string tipoEntrenamiento;
        public float  duracionSegundos;
        public int    puntaje;
        public List<string> erroresCometidos = new List<string>();
    }

    // Wrapper necesario porque JsonUtility no serializa listas en el nivel raiz
    [Serializable]
    internal class TrainingSessionList
    {
        public List<TrainingSession> sessions = new List<TrainingSession>();
    }
}
