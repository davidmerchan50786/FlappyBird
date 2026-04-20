using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(TrailRenderer))]
public class JugadorController : MonoBehaviour
{
    [Header("Movimiento")]
    public float fuerzaSalto = 5f;

    [Header("Animación")]
    public SpriteRenderer spriteRenderer;
    public Sprite spriteAleteo1;          
    public Sprite spriteAleteo2;          
    private bool usandoSprite1 = true;    

    [Header("Efectos de Sonido")]
    public AudioClip clipSalto;
    public AudioClip clipPunto;
    public AudioClip clipGolpe;
    private AudioSource audioSource;      

    [Header("Efectos Visuales")]
    public TrailRenderer estela;          
    public Gradient colorNivel1; 
    public Gradient colorNivel2; 
    public Gradient colorNivel3; 

    private Rigidbody2D rb;               
    private bool muerto = false;          
    
    // Caché del GameManager
    private GameManager gameManagerLocal;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>(); 
        
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (estela == null) estela = GetComponent<TrailRenderer>();
    }

    void Start()
    {
        // Validamos e inicializamos el GameManager en Start
        gameManagerLocal = GameManager.Instance;
        if (gameManagerLocal == null) 
        {
            Debug.LogError("[JugadorController] GameManager no encontrado en la escena.");
        }

        rb.gravityScale = 1f; 
        
        if (spriteRenderer != null && spriteAleteo1 != null)
            spriteRenderer.sprite = spriteAleteo1;

        ConfigurarEstela();
    }

    void ConfigurarEstela()
    {
        if (estela == null) return;
        
        // Evitamos null reference comprobando que GestorNivel existe
        int nivel = GestorNivel.Instance != null ? GestorNivel.Instance.ObtenerNivelActual() : 1;
        
        estela.colorGradient = nivel switch
        {
            1 => colorNivel1,
            2 => colorNivel2,
            3 => colorNivel3,
            _ => colorNivel1 
        };
    }

    void Update()
    {
        if (muerto) return; 

        // Separamos el input de PC del input táctil para mayor seguridad
        bool salto = Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
        
        #if UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            salto = true;
        }
        #endif

        if (salto) Saltar();

        // Limitamos estrictamente el valor entre 0 y 1 para que el Lerp no se vuelva loco
        float t = Mathf.Clamp01((rb.linearVelocity.y + 10f) / 20f);
        float angulo = Mathf.Lerp(-90f, 30f, t);
        transform.rotation = Quaternion.Euler(0f, 0f, angulo);
    }

    void Saltar()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
        AlternarDibujo();
        
        if (clipSalto != null && audioSource != null) 
            audioSource.PlayOneShot(clipSalto); 
    }

    void AlternarDibujo()
    {
        if (spriteAleteo1 == null || spriteAleteo2 == null || spriteRenderer == null) return;
        
        // Lógica corregida: alternamos primero, aplicamos después
        usandoSprite1 = !usandoSprite1;
        spriteRenderer.sprite = usandoSprite1 ? spriteAleteo1 : spriteAleteo2;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (muerto) return;
        
        // Mantenemos CompareTag por eficiencia de memoria en el GC (Garbage Collector)
        if (col.gameObject.CompareTag("Obstaculo") || col.gameObject.CompareTag("Suelo"))
        {
            Morir();
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // Añadimos validación del collider nulo por seguridad
        if (col != null && col.CompareTag("ZonaPuntos"))
        {
            if (gameManagerLocal != null) gameManagerLocal.SumarPuntos(1);
            
            if (clipPunto != null && audioSource != null) 
                audioSource.PlayOneShot(clipPunto); 
        }
    }

    void Morir()
    {
        muerto = true;
        
        if (clipGolpe != null && audioSource != null) 
            audioSource.PlayOneShot(clipGolpe); 
        
        rb.linearVelocity = Vector2.zero; 
        rb.gravityScale = 2f;             
        
        if (gameManagerLocal != null) gameManagerLocal.PerderVida();
    }

    // Método opcional listo por si migras a un sistema de Object Pooling para el jugador
    public void Reiniciar()
    {
        muerto = false;
        rb.gravityScale = 1f;
        rb.linearVelocity = Vector2.zero;
        transform.rotation = Quaternion.identity;
    }
}