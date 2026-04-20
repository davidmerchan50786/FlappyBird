using System.Collections.Generic;
using UnityEngine;

public class SpawnerTuberias : MonoBehaviour
{
    public static SpawnerTuberias Instance { get; private set; }

    [Header("Referencias")]
    public GameObject prefabTuberia;

    [Header("Configuración de Spawn")]
    public float posXSpawn = 10f;
    public float alturaMin = -1.5f;
    public float alturaMax = 2.0f;

    [Header("Configuración Pool")]
    public int tamañoPoolInicial = 5;
    public int limitePoolSeguridad = 15; // MEJORA 3: Evita crecimiento infinito en RAM
    
    private Queue<GameObject> poolTuberias = new Queue<GameObject>();

    [Header("Ajustes de Dificultad")]
    public float velocidadActual { get; private set; }
    private float intervaloSpawn;
    private float temporizador;

    // MEJORA 7: Caché de referencias para no buscar la Instancia en cada frame
    private GameManager gameManagerLocal;
    private GestorNivel gestorNivelLocal;

    void Awake()
    {
        // MEJORA 1: Singleton estricto, pero local a la escena (sin DontDestroyOnLoad)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // MEJORA 4: Validación crítica inicial
        if (prefabTuberia == null)
        {
            Debug.LogError("[SpawnerTuberias] ¡CRÍTICO! No se ha asignado el Prefab de la Tubería.");
            return; 
        }

        // Llenamos la caché
        gameManagerLocal = GameManager.Instance;
        gestorNivelLocal = GestorNivel.Instance;

        ConfigurarDificultad();
        InicializarPool();
        temporizador = intervaloSpawn;
    }

    void ConfigurarDificultad()
    {
        // Validación segura de GestorNivel
        int nivel = gestorNivelLocal != null ? gestorNivelLocal.ObtenerNivelActual() : 1;

        switch (nivel)
        {
            case 1: velocidadActual = 3.5f; intervaloSpawn = 2.5f; break;
            case 2: velocidadActual = 4.5f; intervaloSpawn = 2.0f; break;
            case 3: velocidadActual = 5.5f; intervaloSpawn = 1.6f; break;
            default: velocidadActual = 3.5f; intervaloSpawn = 2.5f; break;
        }
    }

    void InicializarPool()
    {
        for (int i = 0; i < tamañoPoolInicial; i++)
        {
            CrearTuberiaApagada();
        }
    }

    private GameObject CrearTuberiaApagada()
    {
        GameObject tuberia = Instantiate(prefabTuberia, transform);
        tuberia.SetActive(false);
        poolTuberias.Enqueue(tuberia);
        return tuberia;
    }

    void Update()
    {
        if (gameManagerLocal != null && gameManagerLocal.EstaTerminada()) return;

        temporizador -= Time.deltaTime;
        if (temporizador <= 0f)
        {
            SpawnearTuberia();
            temporizador = intervaloSpawn;
        }
    }

    void SpawnearTuberia()
    {
        if (prefabTuberia == null) return;

        GameObject tuberia;

        // Comprobamos si el pool se ha quedado sin objetos
        if (poolTuberias.Count == 0)
        {
            // MEJORA 3: Límite duro para evitar fugas de memoria extremas
            if (transform.childCount < limitePoolSeguridad)
            {
                tuberia = CrearTuberiaApagada();
                tuberia = poolTuberias.Dequeue();
            }
            else
            {
                Debug.LogWarning("[SpawnerTuberias] Límite de seguridad alcanzado. Cancelando spawn para proteger RAM.");
                return;
            }
        }
        else
        {
            tuberia = poolTuberias.Dequeue();
        }

        float alturaAleatoria = Random.Range(alturaMin, alturaMax);
        tuberia.transform.position = new Vector3(posXSpawn, alturaAleatoria, 0);
        tuberia.SetActive(true);
    }

    public void DevolverTuberia(GameObject tuberia)
    {
        tuberia.SetActive(false);
        
        // Evitamos meter la misma tubería dos veces seguidas por accidente
        if (!poolTuberias.Contains(tuberia)) 
        {
            poolTuberias.Enqueue(tuberia);
        }
    }

    void OnDestroy()
    {
        // MEJORA 2: Limpieza absoluta del Pool (Previene Memory Leaks al cambiar de escena)
        poolTuberias.Clear();
    }
}