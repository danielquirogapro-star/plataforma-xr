using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace PlataformaXR.AI
{
    /// <summary>
    /// Tutor inteligente via Claude API. Responde preguntas sobre equipos biomedicos en español.
    /// </summary>
    public class AITutor : MonoBehaviour
    {
        [Header("Configuracion de la API")]
        [Tooltip("API Key de Anthropic - NO hardcodear, asignar desde ProjectConfig")]
        [SerializeField] private string apiKey;
        [SerializeField] private string modelId = "claude-sonnet-4-6";
        [SerializeField] private int maxTokens = 1024;

        private const string ApiEndpoint      = "https://api.anthropic.com/v1/messages";
        private const string AnthropicVersion = "2023-06-01";

        // Prompt del sistema: define el rol y comportamiento del tutor
        private const string SystemPrompt =
            "Eres un tutor experto en equipos biomedicos para hospitales latinoamericanos. " +
            "Tu mision es ensenar a tecnicos y operadores a usar y mantener equipos medicos " +
            "de forma segura y efectiva. Respondes siempre en espanol, con un lenguaje claro, " +
            "didactico y adaptado al nivel del estudiante. " +
            "Cuando expliques procedimientos, usa pasos numerados. " +
            "Cuando identifiques un error del estudiante, corrigelo de forma constructiva. " +
            "Siempre prioriza la seguridad del paciente en tus explicaciones.";

        public event Action<string> OnTutorError;
        public event Action<string> OnResponseReceived;

        /// <summary>
        /// Envia una pregunta al tutor y retorna la respuesta de Claude.
        /// Usa async/await con UnityWebRequest para no bloquear el hilo principal.
        /// </summary>
        public async Task<string> AskTutor(string userQuestion)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Debug.LogError("[AITutor] API Key no configurada.");
                OnTutorError?.Invoke("El tutor no esta configurado correctamente.");
                return null;
            }
            if (string.IsNullOrWhiteSpace(userQuestion)) return null;

            try
            {
                string response = await SendRequest(BuildRequestBody(userQuestion));
                OnResponseReceived?.Invoke(response);
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AITutor] {ex.Message}");
                OnTutorError?.Invoke("Hubo un problema de conexion con el tutor. Intenta de nuevo.");
                return null;
            }
        }

        // Construye el JSON del cuerpo de la peticion segun la API de Anthropic
        private string BuildRequestBody(string question)
        {
            string q = question.Replace("\\","\\\\").Replace("\"","\\\"").Replace("\n","\\n").Replace("\r","\\r");
            return $"{{\"model\":\"{modelId}\",\"max_tokens\":{maxTokens},\"system\":\"{EscapeForJson(SystemPrompt)}\",\"messages\":[{{\"role\":\"user\",\"content\":\"{q}\"}}]}}";
        }

        // Usa TaskCompletionSource para adaptar la coroutine de UnityWebRequest a async/await
        private Task<string> SendRequest(string body)
        {
            var tcs = new TaskCompletionSource<string>();
            StartCoroutine(SendRequestCoroutine(body, tcs));
            return tcs.Task;
        }

        private IEnumerator SendRequestCoroutine(string body, TaskCompletionSource<string> tcs)
        {
            using var req = new UnityWebRequest(ApiEndpoint, "POST");
            req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type",      "application/json");
            req.SetRequestHeader("x-api-key",         apiKey);
            req.SetRequestHeader("anthropic-version", AnthropicVersion);

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
                tcs.SetResult(ParseResponseContent(req.downloadHandler.text));
            else
            {
                Debug.LogError($"[AITutor] HTTP {req.responseCode}: {req.downloadHandler.text}");
                tcs.SetException(new Exception($"HTTP {req.responseCode}: {req.error}"));
            }
        }

        // Extrae el campo "text" del JSON de respuesta de Anthropic sin dependencias externas
        private string ParseResponseContent(string json)
        {
            const string marker = "\"text\":\"";
            int start = json.IndexOf(marker, StringComparison.Ordinal);
            if (start == -1) return "No pude procesar la respuesta del tutor.";
            start += marker.Length;
            int end = start;
            while (end < json.Length) { if (json[end] == '"' && json[end-1] != '\\') break; end++; }
            return json.Substring(start, end - start)
                .Replace("\\n","\\n").Replace("\\r","\r").Replace("\\\"","\"").Replace("\\\\","\\");
        }

        private string EscapeForJson(string s) =>
            s.Replace("\\","\\\\").Replace("\"","\\\"").Replace("\n","\\n").Replace("\r","\\r");

        /// <summary>Configura la API key desde codigo (llamado por ProjectConfig al iniciar).</summary>
        public void SetApiKey(string key) => apiKey = key;
    }
}
