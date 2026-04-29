using UnityEngine;
using System;

// Simula la señal ECG en tiempo real. Compatible con Unity 6 URP.
public class ECGSimulator : MonoBehaviour
{
    [Header("Configuracion ECG")]
    public int frecuenciaCardiaca = 72;
    public bool ritmoNormal = true;
    public float amplitud = 1f;

    public event Action<float> OnFrecuenciaCambiada;

    void Update()
    {
        float punto = GenerarPuntoECG(Time.time);
    }

    public float GenerarPuntoECG(float tiempo)
    {
        float frecuencia = frecuenciaCardiaca / 60f;
        float t = (tiempo * frecuencia) % 1f;
        float valor = 0f;

        // Onda P
        if (t < 0.1f)
            valor = amplitud * 0.25f * Mathf.Sin(t / 0.1f * Mathf.PI);
        // Complejo QRS
        else if (t < 0.15f)
            valor = -amplitud * 0.1f;
        else if (t < 0.175f)
            valor = amplitud * 1.0f * Mathf.Sin((t - 0.15f) / 0.025f * Mathf.PI);
        else if (t < 0.2f)
            valor = -amplitud * 0.3f;
        // Onda T
        else if (t < 0.4f)
            valor = amplitud * 0.35f * Mathf.Sin((t - 0.2f) / 0.2f * Mathf.PI);

        if (!ritmoNormal)
            valor += UnityEngine.Random.Range(-0.2f, 0.2f);

        return valor;
    }

    public void SimularArritmia()
    {
        ritmoNormal = false;
        frecuenciaCardiaca = UnityEngine.Random.Range(40, 140);
        OnFrecuenciaCambiada?.Invoke(frecuenciaCardiaca);
    }

    public void RestaurarRitmoNormal()
    {
        ritmoNormal = true;
        frecuenciaCardiaca = 72;
        OnFrecuenciaCambiada?.Invoke(frecuenciaCardiaca);
    }
}