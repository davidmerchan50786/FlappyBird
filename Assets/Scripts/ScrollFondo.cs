using UnityEngine;

// Hace que el fondo se mueva en bucle infinito hacia la izquierda
// Ponlo en el GameObject del fondo — necesita un SpriteRenderer
//
// Cómo configurarlo en Unity:
// 1. Crea dos GameObjects con el mismo sprite de fondo (uno al lado del otro)
// 2. Pon este script en el primero
// 3. Arrastra el segundo al campo "fondoCopia" en el Inspector
//
public class ScrollFondo : MonoBehaviour
{
    public float velocidad = 1f;

    [Header("Copia del fondo/suelo (para el efecto infinito)")]
    public Transform fondoCopia;   // El segundo sprite, posicionado justo a la derecha

    [Header("Opciones")]
    [Tooltip("Márcalo en TRUE en el fondo para que siempre se mueva.\n" +
             "Déjalo en FALSE en el suelo para que se pare cuando el pájaro muere.")]
    public bool moverEnGameOver = false;  // Fondo: true — Suelo: false

    // Ancho del sprite — lo medimos al inicio para saber cuándo teletransportar
    private float anchura;

    // Caché del GameManager
    private GameManager gameManagerLocal;

    void Start()
    {
        gameManagerLocal = GameManager.Instance;

        // Medimos el ancho real del sprite (ya escalado) para calcular el bucle
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            anchura = sr.bounds.size.x;

        // Colocamos la copia justo al lado derecho del fondo original
        if (fondoCopia != null)
            fondoCopia.position = new Vector3(
                transform.position.x + anchura,
                transform.position.y,
                transform.position.z
            );
    }

    void Update()
    {
        // Suelo (moverEnGameOver = false): se para solo cuando se acaban las vidas (pantalla de game over)
        //   Durante el menú y el respawn el suelo sigue moviéndose — igual que el original
        // Fondo (moverEnGameOver = true): se mueve siempre sin excepción
        if (!moverEnGameOver && gameManagerLocal != null && gameManagerLocal.EstaJuegoTerminado())
            return;

        // Movemos tanto el sprite original como su copia
        Mover(transform);
        if (fondoCopia != null) Mover(fondoCopia);
    }

    void Mover(Transform t)
    {
        // Desplazamos hacia la izquierda
        t.Translate(Vector3.left * velocidad * Time.deltaTime);

        // Cuando el sprite sale completamente por la izquierda,
        // lo mandamos al final de la cola para que el bucle sea infinito
        if (t.position.x <= -anchura)
            t.position += new Vector3(anchura * 2f, 0f, 0f);
    }
}
