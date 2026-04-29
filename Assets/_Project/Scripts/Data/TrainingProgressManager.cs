using UnityEngine;
using System;
using System.Collections.Generic;

// Maneja el progreso del entrenamiento con PlayerPrefs
public class TrainingProgressManager : MonoBehaviour
{
    [Serializable]
    public class SesionEntrenamiento
    {
        public string fecha;
        public float duracion;
        public int puntaje;
        public int errores;
    }

    private List<SesionEntrenamiento> historial = new List<SesionEntrenamiento>();

    public void GuardarSesion(float duracion, int errores)
    {
        var sesion = new SesionEntrenamiento
        {
            fecha    = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
            duracion = duracion,
            puntaje  = CalcularPuntaje(duracion, errores),
            errores  = errores
        };
        historial.Add(sesion);
        PlayerPrefs.SetInt("TotalSesiones", historial.Count);
        PlayerPrefs.Save();
    }

    public int CalcularPuntaje(float duracion, int errores)
    {
        int puntajeBase = 100;
        puntajeBase -= errores * 10;
        if (duracion < 300f) puntajeBase += 20;
        return Mathf.Max(0, puntajeBase);
    }

    public List<SesionEntrenamiento> ObtenerHistorial()
    {
        return historial;
    }
}