using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI textoPuntuacionFinal;
    public TextMeshProUGUI textoMejorPuntuacion;
    
    // MEJORA 5: Constantes para evitar errores de tipeo
    private const string MENU_SCENE = "MenuPrincipal";
    
    // MEJORA 8: Retraso para permitir que suene el "clic" del botón o se vea una animación
    private const float DELAY_TRANSICION = 0.2f; 

    void Start()
    {
        // MEJORA 2: Red de seguridad. Garantizamos que el tiempo corra para que 
        // las corrutinas y animaciones de la UI funcionen correctamente.
        Time.timeScale = 1f; 

        // MEJORA 7: Validación de UI
        if (textoPuntuacionFinal == null || textoMejorPuntuacion == null)
        {
            Debug.LogError("[GameOverManager] Elementos UI no asignados en el Inspector.");
        }

        // Leemos los datos (Recuerda: ya se guardaron en disco en GameManager.ActivarGameOver)
        int final = GestorPuntuacion.Instance != null ? GestorPuntuacion.Instance.PuntuacionActual : 0;
        int mejor = GestorPuntuacion.Instance != null ? GestorPuntuacion.Instance.MejorPuntuacion : 0;

        if (textoPuntuacionFinal != null) textoPuntuacionFinal.text = $"Puntos: {final}";
        if (textoMejorPuntuacion != null) textoMejorPuntuacion.text = $"Mejor: {mejor}";
    }

    public void AlPulsarReintentar()
    {
        // Desactivamos la interactividad de los botones aquí si tuvieras CanvasGroup
        StartCoroutine(RutinaReintentar());
    }

    private IEnumerator RutinaReintentar()
    {
        // Esperamos un instante para el feedback visual/sonoro del botón
        yield return new WaitForSeconds(DELAY_TRANSICION);
        
        // El GameManager ya se encarga de reiniciar todo (Vidas, Puntos, Nivel) 
        // y de recargar la escena desde cero (limpiando Spawners y Pájaro).
        if (GameManager.Instance != null)
        {
            GameManager.Instance.IniciarPartida();
        }
        else
        {
            Debug.LogError("[GameOverManager] Falla crítica: No hay GameManager.");
        }
    }

    public void AlPulsarMenu()
    {
        StartCoroutine(RutinaVolverAlMenu());
    }

    private IEnumerator RutinaVolverAlMenu()
    {
        yield return new WaitForSeconds(DELAY_TRANSICION);
        
        // MEJORA 10: Limpiamos el estado visual/lógico antes de volver al menú
        GestorNivel.Instance?.ReiniciarProgreso();
        GestorPuntuacion.Instance?.ReiniciarPuntuacion();
        
        if (Application.CanStreamedLevelBeLoaded(MENU_SCENE))
        {
            SceneManager.LoadScene(MENU_SCENE);
        }
        else
        {
            Debug.LogError($"[GameOverManager] La escena '{MENU_SCENE}' no está en Build Settings.");
        }
    }
}