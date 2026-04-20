using UnityEngine;

// Gestiona la música de fondo — cambia la canción según la pantalla en la que estés
// Ponlo en el mismo GameObject que el GameManager (o en uno propio)
// Necesita un AudioSource en el mismo objeto
[RequireComponent(typeof(AudioSource))]
public class GestorAudio : MonoBehaviour
{
    [Header("Música por pantalla")]
    public AudioClip musicaMenu;        // Suena en la pantalla de bienvenida
    public AudioClip musicaJuego;       // Suena durante la partida
    public AudioClip musicaGameOver;    // Suena en la pantalla de game over

    [Header("Volumen")]
    [Range(0f, 1f)] public float volumenMusica = 0.4f;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // La música va en bucle siempre
        audioSource.loop   = true;
        audioSource.volume = volumenMusica;
    }

    // Cambia la canción que suena. Si ya está sonando esa misma, no hacemos nada.
    public void CambiarMusica(AudioClip nuevaMusica)
    {
        if (nuevaMusica == null) return;

        // Evitamos cortar y volver a poner la misma canción
        if (audioSource.clip == nuevaMusica) return;

        audioSource.clip = nuevaMusica;
        audioSource.Play();
    }

    // Métodos de acceso rápido — el GameManager los llama según la pantalla
    public void PonerMusicaMenu()     => CambiarMusica(musicaMenu);
    public void PonerMusicaJuego()    => CambiarMusica(musicaJuego);
    public void PonerMusicaGameOver() => CambiarMusica(musicaGameOver);
}
