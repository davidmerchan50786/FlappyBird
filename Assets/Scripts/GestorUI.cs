using UnityEngine;
using TMPro;

public class GestorUI : MonoBehaviour
{
    [Header("Referencias de Textos")]
    public TextMeshProUGUI textoPuntuacion;
    public TextMeshProUGUI textoVidas;
    public TextMeshProUGUI textoNivel; // Opcional, por si añades un texto de nivel

    void OnEnable()
    {
        // Nos suscribimos de forma segura usando +=
        EventosUI.OnPuntuacionCambiada += ActualizarPuntos;
        EventosUI.OnVidasCambiadas += ActualizarVidas;
        EventosUI.OnNivelCambiado += ActualizarNivel;
    }

    void OnDisable()
    {
        // CRÍTICO: Previene el Memory Leak desuscribiéndonos antes de que el objeto muera
        EventosUI.OnPuntuacionCambiada -= ActualizarPuntos;
        EventosUI.OnVidasCambiadas -= ActualizarVidas;
        EventosUI.OnNivelCambiado -= ActualizarNivel;
    }

    void Start()
    {
        // Pedimos los datos iniciales reales a los Gestores (por si la UI carga un frame más tarde)
        int puntos = GestorPuntuacion.Instance != null ? GestorPuntuacion.Instance.PuntuacionActual : 0;
        int vidas = GameManager.Instance != null ? GameManager.Instance.ObtenerVidas() : 3;
        int nivel = GestorNivel.Instance != null ? GestorNivel.Instance.ObtenerNivelActual() : 1;
        
        ActualizarPuntos(puntos);
        ActualizarVidas(vidas);
        ActualizarNivel(nivel);
    }

    private void ActualizarPuntos(int puntos)
    {
        if (textoPuntuacion != null) textoPuntuacion.text = puntos.ToString();
    }

    private void ActualizarVidas(int vidas)
    {
        if (textoVidas != null) textoVidas.text = $"Vidas: {vidas}";
    }

    private void ActualizarNivel(int nivel)
    {
        // Si no tienes este texto en la UI, no pasa nada, simplemente ignora la actualización
        if (textoNivel != null) textoNivel.text = $"Nivel: {nivel}";
    }
}