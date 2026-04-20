using UnityEngine;

// Script del jugador — va en el GameObject del pájaro
// Necesita Rigidbody2D, AudioSource y TrailRenderer en el mismo objeto
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(TrailRenderer))]
public class JugadorController : MonoBehaviour
{
    // ---- MOVIMIENTO ----
    [Header("Movimiento")]
    public float fuerzaSalto = 5f;

    // ---- ANIMACIÓN ----
    // Arrastra aquí los sprites del pájaro en orden (Bird1-1, Bird1-2 ... Bird1-7)
    // El pájaro aletea automáticamente sin necesidad de hacer clic
    [Header("Animación")]
    public SpriteRenderer spriteRenderer;       // El SpriteRenderer del pájaro
    public Sprite[]       spritesAleteo;        // Todos los fotogramas de la animación en orden
    public float          fotogramasPorSegundo = 10f; // Velocidad del aleteo

    private int   fotogramaActual  = 0;         // Fotograma que se muestra ahora
    private float temporizadorAnim = 0f;        // Tiempo acumulado entre fotogramas

    // ---- TRAIL (estela que deja el pájaro al moverse) ----
    [Header("Efectos Visuales")]
    public TrailRenderer estela;                // Arrastrarlo desde el Inspector

    // ---- AUDIO ----
    [Header("Efectos de Sonido")]
    public AudioClip clipSalto;                 // Sonido al saltar
    public AudioClip clipPunto;                 // Sonido al pasar una tubería (opcional)
    public AudioClip clipGolpe;                 // Sonido al morir
    private AudioSource audioSource;

    // ---- VARIABLES INTERNAS ----
    private Rigidbody2D  rb;
    private bool         muerto = false;

    // Caché del GameManager para no llamar a Instance en cada frame
    private GameManager gameManagerLocal;

    // ======================================================

    void Awake()
    {
        // Recogemos los componentes en Awake para que estén listos antes que Start del GameManager
        rb          = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();

        // Si no arrastraste el SpriteRenderer o el Trail desde el Inspector,
        // los buscamos automáticamente en el mismo objeto
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (estela         == null) estela         = GetComponent<TrailRenderer>();
    }

    void Start()
    {
        gameManagerLocal = GameManager.Instance;
        AplicarFotograma();
    }

    void Update()
    {
        if (muerto) return;

        // Saltamos con Espacio, clic del ratón o toque en pantalla (móvil/WebGL)
        bool salto = Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
        if (salto) Saltar();

        // Rotamos el pájaro según si va hacia arriba o hacia abajo
        float angulo = Mathf.Clamp(rb.linearVelocity.y * 5f, -90f, 30f);
        transform.rotation = Quaternion.Euler(0f, 0f, angulo);

        // Avanzamos la animación de aleteo
        AnimarAleteo();
    }

    void Saltar()
    {
        // Cancelamos la velocidad actual y aplicamos el impulso hacia arriba
        rb.linearVelocity = new Vector2(0f, fuerzaSalto);

        if (clipSalto != null)
            audioSource.PlayOneShot(clipSalto);
    }

    // Cicla los fotogramas automáticamente según fotogramasPorSegundo
    void AnimarAleteo()
    {
        if (spritesAleteo == null || spritesAleteo.Length == 0) return;

        temporizadorAnim += Time.deltaTime;

        float tiempoEntreFotogramas = 1f / fotogramasPorSegundo;
        if (temporizadorAnim >= tiempoEntreFotogramas)
        {
            temporizadorAnim -= tiempoEntreFotogramas;
            fotogramaActual   = (fotogramaActual + 1) % spritesAleteo.Length;
            AplicarFotograma();
        }
    }

    void AplicarFotograma()
    {
        if (spriteRenderer == null || spritesAleteo == null || spritesAleteo.Length == 0) return;
        spriteRenderer.sprite = spritesAleteo[fotogramaActual];
    }

    // Se llama cuando el jugador entra en un trigger (la ZonaPuntos en el hueco de la tubería)
    // La ZonaPuntos debe tener el tag "ZonaPuntos" y "Is Trigger" marcado en su BoxCollider2D
    void OnTriggerEnter2D(Collider2D col)
    {
        if (muerto) return;

        if (col.CompareTag("ZonaPuntos"))
        {
            // Reproducimos el sonido de punto (opcional)
            if (clipPunto != null)
                audioSource.PlayOneShot(clipPunto);

            // Avisamos al GameManager para que sume el punto y compruebe nivel
            gameManagerLocal?.SumarPuntos();
        }
    }

    // Se llama automáticamente cuando choca con un collider normal (no trigger)
    void OnCollisionEnter2D(Collision2D col)
    {
        if (muerto) return;

        if (col.gameObject.CompareTag("Obstaculo") || col.gameObject.CompareTag("Suelo"))
            Morir();
    }

    void Morir()
    {
        muerto = true;

        // Paramos el movimiento para que se quede quieto al morir
        rb.linearVelocity = Vector2.zero;

        if (clipGolpe != null)
            audioSource.PlayOneShot(clipGolpe);

        // Avisamos al GameManager — él decide si queda vida o es game over
        if (gameManagerLocal != null)
            gameManagerLocal.PerderVida();
    }

    // El GameManager llama a este método para volver a poner al jugador en marcha
    public void Reiniciar()
    {
        muerto = false;
        rb.linearVelocity  = Vector2.zero;
        transform.rotation = Quaternion.identity;
        transform.position = new Vector3(-2f, 0f, 0f);

        // Reiniciamos la animación desde el primer fotograma
        fotogramaActual  = 0;
        temporizadorAnim = 0f;
        AplicarFotograma();

        // Limpiamos la estela para que no quede rastro de la muerte anterior
        if (estela != null) estela.Clear();
    }
}
