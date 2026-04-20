using UnityEngine;

public class GestorPuntuacion : MonoBehaviour
{
    public static GestorPuntuacion Instance { get; private set; }
    
    public int PuntuacionActual { get; private set; }
    public int MejorPuntuacion  { get; private set; }
    
    private const string MEJOR_PUNTUACION_KEY = "MejorPuntuacion";
    private const int MAX_PUNTOS = int.MaxValue - 1000;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        CargarMejorPuntuacion();
    }

    private void CargarMejorPuntuacion()
    {
        try
        {
            MejorPuntuacion = PlayerPrefs.GetInt(MEJOR_PUNTUACION_KEY, 0);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Error al cargar mejor puntuación: {ex.Message}");
            MejorPuntuacion = 0;
        }
    }

    public void SumarPuntos(int puntos)
    {
        // Protección contra números negativos y desbordamiento de enteros
        if (puntos < 0 || PuntuacionActual + puntos > MAX_PUNTOS)
        {
            Debug.LogWarning($"Intento inválido de sumar puntos: {puntos}");
            return;
        }
        
        PuntuacionActual += puntos;
        
        // CORRECCIÓN: Usamos el método seguro del bus de eventos
        EventosUI.InvocarPuntuacionCambiada(PuntuacionActual);
    }

    public void ReiniciarPuntuacion()
    {
        PuntuacionActual = 0;
        
        // CORRECCIÓN: Usamos el método seguro del bus de eventos
        EventosUI.InvocarPuntuacionCambiada(0);
    }

    // Se llama solo al morir para ahorrar operaciones de escritura en disco
    public void GuardarMejorPuntuacion()
    {
        if (PuntuacionActual > MejorPuntuacion)
        {
            MejorPuntuacion = PuntuacionActual;
            try
            {
                PlayerPrefs.SetInt(MEJOR_PUNTUACION_KEY, MejorPuntuacion);
                PlayerPrefs.Save();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error al guardar mejor puntuación en disco: {ex.Message}");
            }
        }
    }
}