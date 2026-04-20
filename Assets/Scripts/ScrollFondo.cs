using UnityEngine;

public class ScrollFondo : MonoBehaviour 
{
    [Header("Configuración scroll")]
    public float velocidad = 0.5f;
    
    [Header("Comportamiento")]
    public bool moverEnGameOver = false;

    [Header("Duplicado para bucle")]
    public Transform fondoCopia; 

    private float anchura;
    
    // Caché para optimizar el Update
    private GameManager gameManagerLocal;

    void Start()
    {
        gameManagerLocal = GameManager.Instance;

        // Como GestorFondo ya escaló la imagen en Awake, esta anchura es 100% precisa
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            anchura = sr.bounds.size.x;
        }

        // Posicionamos la copia justo al lado para el efecto infinito
        if (fondoCopia != null)
        {
            fondoCopia.position = new Vector3(
                transform.position.x + anchura,
                transform.position.y,
                transform.position.z
            );
        }
    }

    void Update()
    {
        // Usamos la caché local para no penalizar el rendimiento
        if (!moverEnGameOver && gameManagerLocal != null && gameManagerLocal.EstaTerminada()) 
            return;

        Mover(transform);
        if (fondoCopia != null) Mover(fondoCopia);
    }

    void Mover(Transform t)
    {
        t.Translate(Vector3.left * velocidad * Time.deltaTime);

        // Si se sale de la pantalla, lo teletransportamos al final de la cola
        if (t.position.x <= -anchura)
        {
            t.position += new Vector3(anchura * 2f, 0f, 0f);
        }
    }
}