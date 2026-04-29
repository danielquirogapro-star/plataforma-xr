using UnityEngine;
using UnityEngine.Events;

// Controlador principal del Monitor de Signos Vitales.
// Coordina ECGSimulator y SpO2Simulator y gestiona el estado del equipo.
public class VitalSignsMonitor : MonoBehaviour
{
    [Header("Simuladores de senales")]
    public ECGSimulator ecgSimulator;
    public SpO2Simulator spo2Simulator;

    [Header("Estado del monitor")]
    public bool estaEncendido = false;
    public bool modoDemo = false;
    public bool alarmaActiva = false;

    [Header("Eventos de UI")]
    public UnityEvent<float, float, int> onDisplayActualizado;
    public UnityEvent<string> onAlarmaActivada;
    public UnityEvent onAlarmaLimpiada;

    private float _tiempoECG = 0f;

    private void Awake()
    {
        if (spo2Simulator != null)
            spo2Simulator.OnAlarmaActivada += ManejarAlarmaSpO2;
        if (ecgSimulator != null)
            ecgSimulator.OnFrecuenciaCambiada += ManejarCambioFrecuencia;
    }

    private void OnDestroy()
    {
        if (spo2Simulator != null)
            spo2Simulator.OnAlarmaActivada -= ManejarAlarmaSpO2;
        if (ecgSimulator != null)
            ecgSimulator.OnFrecuenciaCambiada -= ManejarCambioFrecuencia;
    }

    private void Update()
    {
        if (!estaEncendido) return;
        _tiempoECG += Time.deltaTime;
        ActualizarDisplay();
    }

    public void ActualizarDisplay()
    {
        if (ecgSimulator == null || spo2Simulator == null) return;
        float valorECG  = ecgSimulator.GenerarPuntoECG(_tiempoECG);
        float valorSpO2 = spo2Simulator.ObtenerLectura();
        onDisplayActualizado?.Invoke(valorECG, valorSpO2, ecgSimulator.frecuenciaCardiaca);
    }

    public void Encender()
    {
        estaEncendido = true;
        _tiempoECG = 0f;
        Debug.Log("[VitalSignsMonitor] Monitor encendido.");
    }

    public void Apagar()
    {
        estaEncendido = false;
        alarmaActiva = false;
        Debug.Log("[VitalSignsMonitor] Monitor apagado.");
    }

    public void ActivarModoDemo()
    {
        modoDemo = true;
        if (ecgSimulator != null) { ecgSimulator.frecuenciaCardiaca = 72; ecgSimulator.ritmoNormal = true; ecgSimulator.amplitud = 1f; }
        if (spo2Simulator != null) { spo2Simulator.spo2Actual = 98f; spo2Simulator.esHipoxia = false; }
        if (!estaEncendido) Encender();
    }

    public void DesactivarModoDemo() => modoDemo = false;

    private void ManejarAlarmaSpO2()
    {
        alarmaActiva = true;
        onAlarmaActivada?.Invoke($"ALARMA SpO2: {spo2Simulator.spo2Actual:F1}% - Nivel critico");
    }

    private void ManejarCambioFrecuencia(float fc) =>
        Debug.Log($"[VitalSignsMonitor] FC: {fc} BPM");
}