using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Threading.Tasks;
using System.Text;

// Tutor inteligente usando Claude API
public class AITutor : MonoBehaviour
{
    [Header("Configuracion API")]
    [SerializeField] private string apiKey = "";

    private const string API_URL = "https://api.anthropic.com/v1/messages";

    private const string SYSTEM_PROMPT =
        "Eres un tutor experto en equipos biomedicos para " +
        "hospitales latinoamericanos. Respondes en espanol, " +
        "eres claro, didactico y practico. Explicas el uso " +
        "y mantenimiento de monitores de signos vitales.";

    public Task<string> PreguntarTutor(string pregunta)
    {
        var tcs = new TaskCompletionSource<string>();
        StartCoroutine(EnviarPregunta(pregunta, tcs));
        return tcs.Task;
    }

    private IEnumerator EnviarPregunta(string pregunta, TaskCompletionSource<string> tcs)
    {
        string escapedPregunta = pregunta.Replace("\"", "\\\"").Replace("\n", "\\n");
        string escapedSystem = SYSTEM_PROMPT.Replace("\"", "\\\"");
        string json = "{" +
            "\"model\":\"claude-sonnet-4-6\"," +
            "\"max_tokens\":1000," +
            "\"system\":\"" + escapedSystem + "\"," +
            "\"messages\":[{\"role\":\"user\",\"content\":\"" + escapedPregunta + "\"}]" +
            "}";

        byte[] datos = Encoding.UTF8.GetBytes(json);

        using var request = new UnityWebRequest(API_URL, "POST");
        request.uploadHandler = new UploadHandlerRaw(datos);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("x-api-key", apiKey);
        request.SetRequestHeader("anthropic-version", "2023-06-01");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            tcs.SetResult(request.downloadHandler.text);
        else
            tcs.SetResult("Error: " + request.error);
    }
}