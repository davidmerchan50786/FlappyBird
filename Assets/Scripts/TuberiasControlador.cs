using UnityEngine;

public class TuberiasControlador : MonoBehaviour
{
    private float posXDestruir;
    
    // Caché de los Singletons para no buscarlos en cada frame
    private GameManager gameManagerLocal;
    private SpawnerTuberias spawnerLocal;

    void Start()
    {
        gameManagerLocal = GameManager.Instance;
        spawnerLocal = SpawnerTuberias.Instance;

        // Calculamos el borde izquierdo de la pantalla en base a la cámara actual, 
        // y le restamos 2 unidades extra para que la tubería desaparezca fuera de la vista.
        if (Camera.main != null)
        {
            posXDestruir = Camera.main.ViewportToWorldPoint(Vector3.zero).x - 2f;
        }
        else
        {
            posXDestruir = -12f; // Fallback por seguridad
        }
    }

    void Update()
    {
        // Validación 1: Si el juego terminó o no hay GameManager, nos detenemos
        if (gameManagerLocal != null && gameManagerLocal.EstaTerminada()) return;

        // Validación 2: Si el Spawner existe, leemos su velocidad. Si no, usamos 3.5f por defecto.
        float velocidad = spawnerLocal != null ? spawnerLocal.velocidadActual : 3.5f;

        // Nos movemos
        transform.Translate(Vector3.left * velocidad * Time.deltaTime);

        // Destrucción / Reciclaje
        if (transform.position.x < posXDestruir)
        {
            if (spawnerLocal != null)
            {
                spawnerLocal.DevolverTuberia(gameObject);
            }
            else
            {
                // Si por algún motivo el Spawner murió antes que esta tubería, nos destruimos
                Destroy(gameObject); 
            }
        }
    }
}