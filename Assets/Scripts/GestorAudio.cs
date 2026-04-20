using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para detectar cambios de escena

[RequireComponent(typeof(AudioSource))]
public class GestorAudio : MonoBehaviour
{
    public static GestorAudio Instance { get; private set; }

    [Header("Efectos de Sonido (SFX)")]
    public AudioClip sfxPunto;

    [Header("Canciones por Escena")]
    public AudioClip musicaMenu;
    public AudioClip musicaJuego;
    public AudioClip musicaGameOver;

    [Header("Configuración")]
    [Range(0f, 1f)] public float volumenMusica = 0.3f;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true; 
        audioSource.volume = volumenMusica;
    }

    void OnEnable()
    {
        // Nos suscribimos al evento de Unity que avisa cuando una escena nueva termina de cargar
        SceneManager.sceneLoaded += AlCargarEscena;

        EventosUI.OnPuntoLogrado += ReproducirSonidoPunto; // Escuchamos el punto
    }

    void OnDisable()
    {
        // Nos desuscribimos para evitar errores si este objeto se destruye
        SceneManager.sceneLoaded -= AlCargarEscena;

        EventosUI.OnPuntoLogrado -= ReproducirSonidoPunto;
    }

    private void ReproducirSonidoPunto()
    {
        if (sfxPunto != null) audioSource.PlayOneShot(sfxPunto);
    }

    // Este método se ejecuta automáticamente cada vez que entras a una nueva escena
    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        AudioClip nuevaCancion = null;

        // Decidimos qué canción poner según el nombre de la escena
        switch (escena.name)
        {
            case "MenuPrincipal":
                nuevaCancion = musicaMenu;
                break;
            case "EscenaJuego":
            case "Nivel1": 
            case "Nivel2":
            case "Nivel3":
                // Todos los niveles usan la misma canción de acción
                nuevaCancion = musicaJuego; 
                break;
            case "GameOver":
                nuevaCancion = musicaGameOver;
                break;
        }

        // Si la canción que toca es diferente a la que ya está sonando, la cambiamos
        if (nuevaCancion != null && audioSource.clip != nuevaCancion)
        {
            audioSource.clip = nuevaCancion;
            audioSource.Play();
        }
    }
}