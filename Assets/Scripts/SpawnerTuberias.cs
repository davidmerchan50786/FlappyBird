using UnityEngine;

// Script que crea tuberías cada cierto tiempo — ponlo en un GameObject vacío en la escena
// El GameManager cambia 'velocidadActual' e 'intervaloSpawn' según el nivel
public class SpawnerTuberias : MonoBehaviour
{
    // Singleton: TuberiasControlador lo usa para leer la velocidad actual
    public static SpawnerTuberias Instance { get; private set; }

    // Arrastra aquí el prefab de la tubería desde el Inspector
    public GameObject prefabTuberia;

    // Posición X donde aparecen las tuberías (fuera de la pantalla por la derecha)
    public float posXSpawn = 10f;

    // -------------------------------------------------------
    // CONFIGURACIÓN DEL HUECO
    //
    // El prefab de tubería debe tener esta estructura:
    //   TuberiaPrefab (raíz — tiene TuberiasControlador.cs)
    //   ├── TuberiaInferior  (sprite de tubo mirando hacia arriba, BoxCollider2D, tag Obstaculo)
    //   ├── TuberiaSuperior  (sprite de tubo mirando hacia abajo, BoxCollider2D, tag Obstaculo)
    //   └── ZonaPuntos       (BoxCollider2D con Is Trigger marcado, en el hueco del medio)
    //
    // El punto central del prefab (0,0,0) debe coincidir con el CENTRO DEL HUECO.
    // -------------------------------------------------------

    [Header("Tamaño del hueco del prefab")]
    [Tooltip("La mitad de la altura del hueco entre las dos tuberías.\n" +
             "Ejemplo: si el hueco mide 3 unidades, pon 1.5")]
    public float mitadHueco = 1.5f;

    [Header("Márgenes de la pantalla")]
    [Tooltip("Espacio desde el borde inferior de la cámara hasta donde está el suelo")]
    public float margenSuelo  = 1.8f;

    [Tooltip("Espacio libre que dejamos en la parte superior de la pantalla")]
    public float margenArriba = 0.8f;

    // Límites calculados automáticamente en Start()
    private float alturaMin;
    private float alturaMax;

    // -------------------------------------------------------
    // VELOCIDAD E INTERVALO — los cambia GameManager según el nivel
    // -------------------------------------------------------
    [HideInInspector] public float velocidadActual = 3f;
    [HideInInspector] public float intervaloSpawn  = 2.5f;

    private float temporizador = 0f;
    private bool  activo       = false;

    // ======================================================

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CalcularLimitesAltura();
    }

    void CalcularLimitesAltura()
    {
        if (Camera.main == null)
        {
            Debug.LogWarning("[SpawnerTuberias] No se encontró la cámara principal. Usando límites por defecto.");
            alturaMin = -2f;
            alturaMax =  2f;
            return;
        }

        // orthographicSize es LA MITAD de la altura visible de la cámara
        float medioAltura = Camera.main.orthographicSize;

        // Límite inferior: borde de cámara + margen de suelo + mitad del hueco
        alturaMin = -medioAltura + margenSuelo + mitadHueco;

        // Límite superior: borde de cámara - margen arriba - mitad del hueco
        alturaMax = medioAltura - margenArriba - mitadHueco;

        if (alturaMin >= alturaMax)
        {
            Debug.LogError("[SpawnerTuberias] ¡Error! alturaMin >= alturaMax. " +
                           "Revisa mitadHueco, margenSuelo y margenArriba.");
            alturaMin = -1f;
            alturaMax =  1f;
        }
    }

    void Update()
    {
        if (!activo) return;

        temporizador += Time.deltaTime;

        if (temporizador >= intervaloSpawn)
        {
            SpawnearTuberia();
            temporizador = 0f;
        }
    }

    void SpawnearTuberia()
    {
        if (prefabTuberia == null)
        {
            Debug.LogError("[SpawnerTuberias] No hay prefab de tubería asignado.");
            return;
        }

        // Elegimos una altura aleatoria para el CENTRO DEL HUECO dentro de los límites seguros
        float alturaAleatoria = Random.Range(alturaMin, alturaMax);
        Vector3 posicion = new Vector3(posXSpawn, alturaAleatoria, 0f);

        Instantiate(prefabTuberia, posicion, Quaternion.identity);
    }

    // El GameManager llama a esto al iniciar o al respawnear tras perder una vida
    public void Activar()
    {
        activo       = true;
        temporizador = 0f;  // Esperamos el tiempo completo antes de la primera tubería
    }

    // El GameManager llama a esto cuando el pájaro muere
    public void Desactivar()
    {
        activo = false;
    }
}
