using UnityEngine;

// Script de cada tubería — va en el prefab de la tubería (raíz del objeto)
// Se mueve hacia la izquierda leyendo la velocidad actual del SpawnerTuberias
// El punto lo detecta el JugadorController al entrar en ZonaPuntos (tag "ZonaPuntos")
public class TuberiasControlador : MonoBehaviour
{
    // Caché de las referencias para no buscarlas en cada frame
    private GameManager     gameManagerLocal;
    private SpawnerTuberias spawnerLocal;

    // Posición X a partir de la cual destruimos la tubería (fuera de pantalla)
    private float posXDestruir;

    void Start()
    {
        gameManagerLocal = GameManager.Instance;
        spawnerLocal     = SpawnerTuberias.Instance;

        // Calculamos el borde izquierdo de la pantalla y le añadimos margen
        if (Camera.main != null)
            posXDestruir = Camera.main.ViewportToWorldPoint(Vector3.zero).x - 2f;
        else
            posXDestruir = -12f; // Valor seguro si no hay cámara
    }

    void Update()
    {
        // Solo nos movemos si el juego está activo
        if (gameManagerLocal != null && gameManagerLocal.EstaTerminada()) return;

        // Leemos la velocidad directamente del Spawner para estar siempre sincronizados
        float velocidad = spawnerLocal != null ? spawnerLocal.velocidadActual : 3.5f;
        transform.Translate(Vector3.left * velocidad * Time.deltaTime);

        // Cuando sale de la pantalla por la izquierda, la destruimos
        if (transform.position.x < posXDestruir)
            Destroy(gameObject);
    }
}
