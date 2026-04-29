using UnityEngine;

namespace PlataformaXR.Data
{
    /// <summary>
    /// ScriptableObject con la configuracion global del proyecto.
    /// Crear desde el menu: PlataformaXR > Configuracion del Proyecto
    /// IMPORTANTE: No commitear el asset generado — esta en .gitignore.
    /// </summary>
    [CreateAssetMenu(fileName = "ProjectConfig", menuName = "PlataformaXR/Configuracion del Proyecto", order = 0)]
    public class ProjectConfig : ScriptableObject
    {
        [Header("Integracion Claude AI")]
        [Tooltip("API Key de Anthropic - obtener en console.anthropic.com")]
        public string claudeApiKey;

        [Header("Informacion de la aplicacion")]
        public string appVersion      = "0.1.0-MVP";
        public string defaultLanguage = "es-CO";

        [Header("Desarrollo")]
        [Tooltip("Activa logs adicionales y herramientas de debug en el editor")]
        public bool debugMode = false;

        [Header("Configuracion de entrenamiento")]
        public int maxSessionDurationMinutes = 60;

        [Range(0, 100)]
        public int passingScore = 70;
    }
}
