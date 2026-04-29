using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlataformaXR.Interaction
{
    /// <summary>
    /// Clase base para interactuar con equipos biomedicos en XR.
    /// Las clases derivadas implementan el comportamiento especifico de cada equipo.
    /// </summary>
    public abstract class EquipmentInteraction : MonoBehaviour
    {
        [Header("Configuracion del equipo")]
        public string equipmentName = "Equipo Biomedico";
        [TextArea(2,4)] public string equipmentDescription;

        [Header("Pasos de entrenamiento")]
        [Tooltip("Lista ordenada de pasos que el estudiante debe completar")]
        public List<InstructionStep> instructionSteps = new List<InstructionStep>();

        private int _currentStepIndex = 0;
        protected bool isGrabbed          = false;
        protected bool isBeingInspected   = false;

        // Evento disparado cuando el estudiante completa un paso
        public event Action<InstructionStep, int> OnStepCompleted;
        // Evento disparado cuando se completan todos los pasos
        public event Action<int> OnTrainingCompleted;
        // Evento disparado cuando el estudiante comete un error de procedimiento
        public event Action<string> OnProcedureError;

        public InstructionStep CurrentStep =>
            (_currentStepIndex < instructionSteps.Count) ? instructionSteps[_currentStepIndex] : null;

        public int CompletedStepsCount
        {
            get { int c = 0; foreach (var s in instructionSteps) if (s.esCompletado) c++; return c; }
        }

        protected virtual void Awake()
        {
            foreach (var step in instructionSteps) step.esCompletado = false;
        }

        /// <summary>Se llama cuando el estudiante agarra el equipo con la mano/controlador XR.</summary>
        public virtual void OnGrab()    { isGrabbed = true;  Debug.Log($"[EquipmentInteraction] {equipmentName} tomado."); }

        /// <summary>Se llama cuando el estudiante suelta el equipo.</summary>
        public virtual void OnRelease() { isGrabbed = false; Debug.Log($"[EquipmentInteraction] {equipmentName} soltado."); }

        /// <summary>Se llama cuando el estudiante activa el modo de inspeccion.</summary>
        public virtual void OnInspect() { isBeingInspected = true; Debug.Log($"[EquipmentInteraction] Inspeccionando: {equipmentName}"); }

        /// <summary>
        /// Marca el paso actual como completado y avanza al siguiente.
        /// Valida que el orden del procedimiento sea correcto.
        /// </summary>
        public void CompleteStep(string stepId)
        {
            if (CurrentStep == null) return;

            if (CurrentStep.stepId != stepId)
            {
                OnProcedureError?.Invoke($"Paso fuera de orden: intentaste '{stepId}' pero corresponde '{CurrentStep.stepId}'");
                return;
            }

            CurrentStep.esCompletado = true;
            OnStepCompleted?.Invoke(CurrentStep, _currentStepIndex);
            _currentStepIndex++;

            if (_currentStepIndex >= instructionSteps.Count)
                OnTrainingCompleted?.Invoke(CalculateTotalScore());
        }

        /// <summary>Reinicia todos los pasos para repetir el ejercicio.</summary>
        public void ResetTraining()
        {
            _currentStepIndex = 0;
            foreach (var step in instructionSteps) step.esCompletado = false;
        }

        private int CalculateTotalScore()
        {
            int t = 0;
            foreach (var s in instructionSteps) if (s.esCompletado) t += s.puntaje;
            return t;
        }
    }

    /// <summary>
    /// Representa un paso individual en el procedimiento de entrenamiento.
    /// </summary>
    [Serializable]
    public class InstructionStep
    {
        [Tooltip("ID unico del paso para validar el orden del procedimiento")]
        public string stepId;

        [Tooltip("Instruccion para el estudiante")]
        [TextArea(2,3)] public string descripcion;

        public bool esCompletado = false;

        [Range(1,20)] public int puntaje = 10;

        [Tooltip("Pista que el tutor puede mostrar si el estudiante se demora")]
        [TextArea(1,3)] public string pista;
    }
}
