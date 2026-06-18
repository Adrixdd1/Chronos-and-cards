using System;
using System.IO;
using UnityEngine;
using ChronosAndCards.Core;

namespace ChronosAndCards.Data
{
    /// <summary>
    /// Componente encargado de leer archivos de preguntas .md en runtime y cargarlos en el ContentManager.
    /// </summary>
    public class ContentLoader : MonoBehaviour
    {
        [SerializeField] private ContentManager _contentManager;

        /// <summary>Se dispara al cargar cartas en el mazo. Parámetro: total de cartas cargadas.</summary>
        public event Action<int> OnContentLoaded;

        /// <summary>Se dispara al completar el parsing, entregando el reporte detallado.</summary>
        public event Action<ParseResult> OnParseCompleted;

        private readonly MarkdownContentParser _parser = new MarkdownContentParser();

        /// <summary>
        /// Carga un archivo Markdown de preguntas desde la ruta especificada.
        /// </summary>
        /// <param name="filePath">Ruta absoluta del archivo .md en el disco.</param>
        public void LoadFromPath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogError("ContentLoader: La ruta del archivo está vacía.");
                return;
            }

            if (!File.Exists(filePath))
            {
                Debug.LogError($"ContentLoader: El archivo no existe en la ruta: {filePath}");
                return;
            }

            try
            {
                string content = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                LoadFromString(content);
            }
            catch (Exception ex)
            {
                Debug.LogError($"ContentLoader: Error al leer el archivo. Detalle: {ex.Message}");
            }
        }

        /// <summary>
        /// Carga y procesa preguntas a partir de una cadena de texto en memoria.
        /// </summary>
        /// <param name="markdownContent">Texto plano en formato Markdown.</param>
        public void LoadFromString(string markdownContent)
        {
            if (string.IsNullOrEmpty(markdownContent))
            {
                Debug.LogWarning("ContentLoader: Contenido vacío recibido. No se cargará ninguna carta.");
                return;
            }

            ParseResult parseResult = _parser.ParseWithDetails(markdownContent);
            
            // Loguear el resumen del ParseResult
            string summary = parseResult.GetSummary();
            Debug.Log(summary);

            // Cargar en ContentManager si hay cartas utilizables
            if (parseResult.IsUsable)
            {
                if (_contentManager != null)
                {
                    _contentManager.LoadCards(parseResult.Cards);
                }
                else
                {
                    Debug.LogWarning("ContentLoader: ContentManager no asignado. No se pudieron registrar las cartas.");
                }

                OnContentLoaded?.Invoke(parseResult.Cards.Count);
                GameEvents.OnContentLoaded?.Invoke(parseResult.Cards.Count);
            }
            else
            {
                Debug.LogError("ContentLoader: El archivo no contiene ninguna pregunta válida utilizable.");
            }

            OnParseCompleted?.Invoke(parseResult);
        }

        /// <summary>
        /// Abre un panel nativo de selección de archivos en el Unity Editor para importar preguntas.
        /// </summary>
        public void OpenFileDialog()
        {
#if UNITY_EDITOR
            string path = UnityEditor.EditorUtility.OpenFilePanel("Seleccionar archivo de preguntas (.md)", "", "md");
            if (!string.IsNullOrEmpty(path))
            {
                LoadFromPath(path);
            }
#else
            Debug.LogWarning("ContentLoader: OpenFileDialog solo está disponible dentro del Unity Editor.");
#endif
        }
    }
}
