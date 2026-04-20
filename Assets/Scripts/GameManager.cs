using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // Singleton para acceso global
    public static GameManager Instance { get; private set; }

    [Header("Configuración Vidas")]
    public int vidasMaximas = 3;
    private int vidasActuales;

    // Constantes para nombres de escenas (deben coincidir con tus Build Settings)
    private const string ESCENA_JUEGO = "EscenaJuego";
    private const string ESCENA_GAMEOVER = "GameOver";

    private bool juegoTerminado = false;
    private bool jugando = false;
    
    // Referencia para el control de la transición de Game Over
    private Coroutine rutinaGameOver; 

    void Awake()
    {
        // Regla del Singleton: solo puede existir uno
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Persiste entre escenas

        // Inicialización de seguridad
        vidasActuales = vidasMaximas; 
    }

    void OnEnable()
    {
        // Nos suscribimos al evento de carga de escenas de Unity
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    void OnDisable()
    {
        // Limpiamos la suscripción al desactivar el objeto
        SceneManager.sceneLoaded -= AlCargarEscena;
    }

    // --- SOLUCIÓN AL PROBLEMA DE ESCENA PAUSADA ---
    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        // En cuanto entramos a la escena de juego, forzamos que el tiempo fluya
        if (escena.name == ESCENA_JUEGO)
        {
            Time.timeScale = 1f;
            Debug.Log("[GameManager] Tiempo reanudado: 1.0f");
        }
    }

    public void IniciarPartida()
    {
        // 1. Limpieza de memoria: evitamos duplicados en los eventos de la UI
        EventosUI.LimpiarListeners(); 

        // 2. Seguridad: cancelamos cualquier paso a GameOver que estuviera pendiente
        if (rutinaGameOver != null) 
        {
            StopCoroutine(rutinaGameOver);
            rutinaGameOver = null;
        }

        // 3. Validación: ¿existe la escena en el proyecto?
        if (!Application.CanStreamedLevelBeLoaded(ESCENA_JUEGO))
        {
            Debug.LogError($"[GameManager] ¡Error! No se encuentra '{ESCENA_JUEGO}' en Build Settings.");
            return; 
        }

        // 4. Reset de lógica de juego
        vidasActuales = vidasMaximas;
        GestorNivel.Instance?.ReiniciarProgreso(); 
        GestorPuntuacion.Instance?.ReiniciarPuntuacion(); 
        
        juegoTerminado = false;
        jugando = true;
        
        // 5. Carga de escena
        SceneManager.LoadScene(ESCENA_JUEGO);
    }

    public void PerderVida()
    {
        if (juegoTerminado) return;

        vidasActuales--;
        
        // Notificamos a la UI mediante el bus de eventos seguro
        EventosUI.InvocarVidasCambiadas(vidasActuales);

        if (vidasActuales <= 0)
        {
            ActivarGameOver();
        }
        else
        {
            // Recargamos el nivel actual para reintentar
            string escenaActual = SceneManager.GetActiveScene().name;
            if (Application.CanStreamedLevelBeLoaded(escenaActual))
                SceneManager.LoadScene(escenaActual);
        }
    }

    public void SumarPuntos(int puntos)
    {
        if (!jugando || juegoTerminado) return;
        
        // Delegamos la puntuación a su gestor
        GestorPuntuacion.Instance?.SumarPuntos(puntos);
        
        // Evaluamos si toca subir de nivel
        int puntosTotales = GestorPuntuacion.Instance != null ? GestorPuntuacion.Instance.PuntuacionActual : 0;
        GestorNivel.Instance?.EvaluarPuntuacion(puntosTotales);
    }

    private void ActivarGameOver()
    {
        juegoTerminado = true;
        jugando = false;

        // Guardamos récord en disco antes de salir
        GestorPuntuacion.Instance?.GuardarMejorPuntuacion();
        
        // Iniciamos la espera visual antes de cambiar de pantalla
        rutinaGameOver = StartCoroutine(RutinaTransicionGameOver());
    }

    private IEnumerator RutinaTransicionGameOver()
    {
        // Dejamos un tiempo para que el jugador vea el choque (1.5 seg)
        yield return new WaitForSecondsRealtime(1.5f);
        
        if (Application.CanStreamedLevelBeLoaded(ESCENA_GAMEOVER))
        {
            SceneManager.LoadScene(ESCENA_GAMEOVER);
        }
        else
        {
            Debug.LogError($"[GameManager] Escena '{ESCENA_GAMEOVER}' no encontrada.");
        }
    }

    public int ObtenerVidas() => vidasActuales;
    public bool EstaTerminada() => juegoTerminado;
}