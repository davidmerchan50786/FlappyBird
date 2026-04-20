using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipalManager : MonoBehaviour
{
    [Header("Configuración Arquitectura")]
    [Tooltip("Nombre del Prefab en la carpeta Assets/Resources que contiene los gestores.")]
    public string rutaPrefabGestores = "GestoresGlobales"; 

    public void AlPulsarJugar()
    {
        // 1. Aseguramos el flujo del tiempo antes de cualquier otra cosa
        Time.timeScale = 1f;

        // 2. Sistema de instanciación de arquitectura global (Singleton por Prefab)
        if (GameManager.Instance == null)
        {
            // Intentamos cargar el prefab desde la carpeta Resources
            GameObject prefabGestores = Resources.Load<GameObject>(rutaPrefabGestores);
            
            if (prefabGestores != null)
            {
                // Al instanciarlo, el Awake del GameManager se encargará del DontDestroyOnLoad
                Instantiate(prefabGestores);
                Debug.Log("<color=green>[MenuPrincipal]</color> Gestores instanciados con éxito.");
            }
            else
            {
                Debug.LogError($"<color=red>[MenuPrincipal]</color> ¡ERROR! No se encuentra '{rutaPrefabGestores}' en Assets/Resources.");
                return; 
            }
        }

        // 3. Iniciamos la lógica de partida
        // Usamos el operador ? para evitar un crash si el Singleton falló por alguna razón externa
        if (GameManager.Instance != null)
        {
            GameManager.Instance.IniciarPartida();
        }
    }

    public void AlPulsarSalir()
    {
        Debug.Log("[MenuPrincipal] Saliendo del juego...");
        
        #if UNITY_EDITOR
            // Si estamos en el editor de Unity, detenemos el Play Mode
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // Si es el juego compilado (EXE o APK), cerramos la aplicación
            Application.Quit();
        #endif
    }
}