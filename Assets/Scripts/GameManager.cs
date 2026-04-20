using UnityEngine;

// El cerebro del juego — controla vidas, niveles, puntuación y qué pantalla mostrar
// Ponlo en un GameObject vacío llamado "GameManager" en la escena
public class GameManager : MonoBehaviour
{
    // Singleton: cualquier script puede usar GameManager.Instance para hablar con este
    public static GameManager Instance { get; private set; }

    // ---- CONFIGURACIÓN (ajustable desde el Inspector) ----

    [Header("Vidas")]
    public int vidasMaximas = 3;

    [Header("Puntos para subir de nivel")]
    public int puntosNivel2 = 10;   // Con 10 puntos pasamos al nivel 2
    public int puntosNivel3 = 20;   // Con 20 puntos pasamos al nivel 3

    // ---- DATOS DE LA PARTIDA ACTUAL ----

    // JuegoActivo: true solo mientras la partida está en marcha
    public bool JuegoActivo    { get; private set; } = false;

    // JuegoTerminado: true solo cuando se acaban todas las vidas (no en menú ni respawn)
    public bool JuegoTerminado { get; private set; } = false;

    public int puntuacion      { get; private set; }
    public int vidas           { get; private set; }
    public int nivel           { get; private set; }
    public int mejorPuntuacion { get; private set; }

    // Clave para guardar el récord en el disco (PlayerPrefs)
    private const string CLAVE_RECORD = "MejorPuntuacion";

    // ---- REFERENCIAS (arrástralas desde el Inspector) ----

    [Header("Referencias")]
    public JugadorController jugador;
    public SpawnerTuberias   spawnerTuberias;
    public GestorUI          gestorUI;
    public GestorAudio       gestorAudio;    // Opcional — sin música el juego funciona igual
    public GestorFondo       gestorFondo;    // Cambia el sprite del fondo según pantalla y nivel
    public ScrollFondo       scrollSuelo;   // El suelo usa el mismo script que el fondo

    // ======================================================
    //  INICIALIZACIÓN
    // ======================================================

    void Awake()
    {
        Instance = this;

        // Cargamos el mejor récord guardado en el disco (0 si nunca se ha jugado)
        mejorPuntuacion = PlayerPrefs.GetInt(CLAVE_RECORD, 0);
    }

    void Start()
    {
        // Al abrir el juego mostramos la pantalla de bienvenida
        MostrarMenuPrincipal();
    }

    // ======================================================
    //  PANTALLA DE BIENVENIDA
    // ======================================================

    void MostrarMenuPrincipal()
    {
        JuegoActivo    = false;
        JuegoTerminado = false;
        spawnerTuberias.Desactivar();
        BorrarTuberias();

        // Escondemos el pájaro hasta que el jugador pulse "Jugar"
        jugador.gameObject.SetActive(false);

        // Música y fondo del menú
        gestorAudio?.PonerMusicaMenu();
        gestorFondo?.CambiarAMenu();

        // Solo mostramos el panel del menú, el resto se oculta
        gestorUI.MostrarMenu(true);
        gestorUI.MostrarHUD(false);
        gestorUI.MostrarGameOver(false);
        gestorUI.MostrarPantallaMuerte(false);

        // Mostramos el récord en el menú
        gestorUI.ActualizarMejorPuntuacion(mejorPuntuacion);
    }

    // ======================================================
    //  INICIO DE PARTIDA
    //  Se llama cuando el jugador pulsa "Jugar" en el menú
    // ======================================================

    public void IniciarPartida()
    {
        // Reseteamos todos los contadores
        puntuacion     = 0;
        vidas          = vidasMaximas;
        nivel          = 1;
        JuegoTerminado = false;

        // Ajustamos la dificultad al nivel 1
        ConfigurarNivel();

        // Ponemos al jugador en su sitio
        jugador.gameObject.SetActive(true);
        jugador.Reiniciar();

        // Limpiamos las tuberías que pudieran haber quedado de la partida anterior
        BorrarTuberias();

        // Actualizamos la UI: ocultamos menú y game over, mostramos el HUD de juego
        gestorUI.MostrarMenu(false);
        gestorUI.MostrarGameOver(false);
        gestorUI.MostrarPantallaMuerte(false);
        gestorUI.MostrarHUD(true);
        gestorUI.ActualizarPuntuacion(0);
        gestorUI.ActualizarVidas(vidas);
        gestorUI.ActualizarNivel(nivel);

        // Música del nivel 1 (ConfigurarNivel ya actualiza el fondo al nivel correcto)
        gestorAudio?.PonerMusicaJuego();

        // Arrancamos las tuberías y marcamos el juego como activo
        spawnerTuberias.Activar();
        JuegoActivo = true;
    }

    // ======================================================
    //  SUMAR PUNTOS
    //  Se llama desde TuberiasControlador cuando el jugador pasa la zona de puntos
    // ======================================================

    public void SumarPuntos(int puntos = 1)
    {
        // Si el juego no está activo, ignoramos — por si llega tarde algún trigger
        if (!JuegoActivo) return;

        puntuacion += puntos;
        gestorUI.ActualizarPuntuacion(puntuacion);

        // Comprobamos si con esta puntuación hay que cambiar de nivel
        int nivelNuevo = CalcularNivel();
        if (nivelNuevo > nivel)
        {
            nivel = nivelNuevo;
            ConfigurarNivel();                    // Cambiamos velocidad e intervalo
            gestorUI.ActualizarNivel(nivel);      // Actualizamos el texto de nivel
            gestorUI.AvisarSubidaNivel(nivel);    // Mostramos "¡NIVEL 2!" en pantalla
        }
    }

    // ======================================================
    //  PÉRDIDA DE VIDA
    //  Se llama desde JugadorController cuando choca con algo
    // ======================================================

    public void PerderVida()
    {
        // Doble protección: si ya no estamos jugando, ignoramos
        if (!JuegoActivo) return;

        JuegoActivo = false;
        spawnerTuberias.Desactivar();

        // Quitamos una vida y actualizamos la UI
        vidas--;
        gestorUI.ActualizarVidas(vidas);

        if (vidas <= 0)
        {
            // --- Sin vidas: guardamos récord y mostramos pantalla de GAME OVER ---
            JuegoTerminado = true;
            GuardarRecord();
            gestorAudio?.PonerMusicaGameOver();
            gestorFondo?.CambiarAGameOver();
            gestorUI.MostrarGameOver(true);
            gestorUI.MostrarPuntuacionFinal(puntuacion, mejorPuntuacion);
        }
        else
        {
            // --- Queda vida: mostramos pantalla de "perdiste una vida"
            // y en 1.5 segundos volvemos a jugar automáticamente ---
            BorrarTuberias();
            gestorUI.MostrarPantallaMuerte(true);
            Invoke(nameof(RespawnTrasVida), 1.5f);
        }
    }

    // Se llama automáticamente 1.5s después de perder una vida (no game over)
    void RespawnTrasVida()
    {
        gestorUI.MostrarPantallaMuerte(false);
        jugador.Reiniciar();
        spawnerTuberias.Activar();
        JuegoActivo = true;
    }

    // ======================================================
    //  BOTONES DE LA PANTALLA DE GAME OVER
    // ======================================================

    // Botón "Reintentar" — vuelve a empezar desde cero
    public void Reintentar()
    {
        CancelInvoke(); // Por si había un respawn pendiente
        IniciarPartida();
    }

    // Botón "Menú" — vuelve a la pantalla de bienvenida
    public void VolverAlMenu()
    {
        CancelInvoke();
        MostrarMenuPrincipal();
    }

    // ======================================================
    //  MÉTODOS DE CONSULTA (para otros scripts)
    // ======================================================

    public int  ObtenerVidas()        => vidas;
    public bool EstaTerminada()       => !JuegoActivo;   // Lo usa TuberiasControlador (para el movimiento)
    public bool EstaJuegoTerminado()  => JuegoTerminado; // Lo usa ScrollFondo (solo para cuando se acaban las vidas)

    // ======================================================
    //  RÉCORD
    // ======================================================

    void GuardarRecord()
    {
        if (puntuacion > mejorPuntuacion)
        {
            mejorPuntuacion = puntuacion;
            PlayerPrefs.SetInt(CLAVE_RECORD, mejorPuntuacion);
            PlayerPrefs.Save();
        }
    }

    // ======================================================
    //  CONFIGURACIÓN DE NIVELES
    //  Cambia velocidad y frecuencia de tuberías según el nivel
    // ======================================================

    void ConfigurarNivel()
    {
        // Nivel 1 → Fácil:   tuberías lentas, bastante espacio entre ellas
        // Nivel 2 → Medio:   más rápido y más frecuente
        // Nivel 3 → Difícil: muy rápidas y seguidas, hay que ser preciso

        switch (nivel)
        {
            case 1:
                spawnerTuberias.velocidadActual = 3.0f;
                spawnerTuberias.intervaloSpawn  = 2.5f;
                break;
            case 2:
                spawnerTuberias.velocidadActual = 4.5f;
                spawnerTuberias.intervaloSpawn  = 2.0f;
                break;
            case 3:
                spawnerTuberias.velocidadActual = 6.0f;
                spawnerTuberias.intervaloSpawn  = 1.6f;
                break;
        }

        // El suelo se mueve a la misma velocidad que las tuberías para que quede coherente
        if (scrollSuelo != null) scrollSuelo.velocidad = spawnerTuberias.velocidadActual;

        // Cambiamos el fondo según el nivel actual (día → tarde → noche)
        gestorFondo?.CambiarANivel(nivel);
    }

    // Mira la puntuación y devuelve en qué nivel debería estar el jugador
    int CalcularNivel()
    {
        if (puntuacion >= puntosNivel3) return 3;
        if (puntuacion >= puntosNivel2) return 2;
        return 1;
    }

    // Busca y destruye todas las tuberías que hay en la escena
    void BorrarTuberias()
    {
        foreach (GameObject t in GameObject.FindGameObjectsWithTag("Obstaculo"))
            Destroy(t);
    }
}
