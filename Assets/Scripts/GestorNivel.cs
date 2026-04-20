using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GestorNivel : MonoBehaviour
{
    public static GestorNivel Instance { get; private set; }
    
    // MEJORA 6: Evento para que la UI o el Audio reaccionen al cambio de fase
    public Action<int> AlCambiarNivel;

    [Header("Configuración de Progresión")]
    public int puntosPorNivel = 10;
    public int nivelMaximo = 3;

    private int nivelActual = 1;
    
    // MEJORA 7: Persistencia del nivel alcanzado (Opcional, muy útil si hay selector de niveles)
    private const string NIVEL_KEY = "NivelActualGuardado";

    void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Recuperamos el nivel si el jugador cerró la app (por defecto 1)
        nivelActual = PlayerPrefs.GetInt(NIVEL_KEY, 1);
    }

    public void ReiniciarProgreso()
    {
        nivelActual = 1;
        PlayerPrefs.SetInt(NIVEL_KEY, nivelActual);
        PlayerPrefs.Save();
    }

    public void EvaluarPuntuacion(int puntosTotales)
    {
        // MEJORA 2: Prevención de División por Cero (Crash)
        if (puntosPorNivel <= 0)
        {
            Debug.LogError("[GestorNivel] ¡Error Crítico! puntosPorNivel debe ser mayor a 0.");
            return;
        }

        // MEJORA 9: Validación de puntos negativos
        if (puntosTotales < 0) return;

        int nuevoNivel = Mathf.Clamp((puntosTotales / puntosPorNivel) + 1, 1, nivelMaximo);

        if (nuevoNivel > nivelActual)
        {
            nivelActual = nuevoNivel;
            
            // Guardamos el progreso en disco
            PlayerPrefs.SetInt(NIVEL_KEY, nivelActual);
            PlayerPrefs.Save();

            // Avisamos a todos los sistemas (UI, Audio, etc.)
            AlCambiarNivel?.Invoke(nivelActual);

            string nombreEscena = $"Nivel{nivelActual}";

            // MEJORA 3 y 4 (Corregida): Validación real en Build Settings
            if (Application.CanStreamedLevelBeLoaded(nombreEscena))
            {
                try
                {
                    SceneManager.LoadScene(nombreEscena);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[GestorNivel] Excepción al intentar cargar {nombreEscena}: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"[GestorNivel] La escena {nombreEscena} no está añadida en File -> Build Settings. El nivel sube lógicamente, pero la escena no cambia.");
            }
        }
    }

    public int ObtenerNivelActual() => nivelActual;
}