using UnityEngine;
using System;

// Simula saturacion de oxigeno en sangre
public class SpO2Simulator : MonoBehaviour
{
    [Header("Configuracion SpO2")]
    public float spo2Actual = 98f;
    public bool esHipoxia = false;

    public event Action OnAlarmaActivada;

    void Update()
    {
        if (spo2Actual < 90f)
            OnAlarmaActivada?.Invoke();
    }

    public float ObtenerLectura()
    {
        float variacion = UnityEngine.Random.Range(-0.5f, 0.5f);
        return Mathf.Clamp(spo2Actual + variacion, 0f, 100f);
    }

    public void SimularHipoxia(float valorObjetivo)
    {
        esHipoxia = true;
        spo2Actual = Mathf.MoveTowards(spo2Actual, valorObjetivo, Time.deltaTime * 2f);
    }

    public void RestaurarNormal()
    {
        esHipoxia = false;
        spo2Actual = 98f;
    }
}