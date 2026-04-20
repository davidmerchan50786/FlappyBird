using UnityEngine;
using TMPro;

// Controla todo lo que se ve en pantalla: los 4 paneles de la UI
// Ponlo en un GameObject vacío llamado "GestorUI" en la escena
//
// Estructura de Canvas que necesitas crear en Unity:
//   Canvas
//   ├── PanelMenu        → título + récord + botón "JUGAR"
//   ├── PanelHUD         → puntuación + vidas + nivel + aviso de nivel
//   ├── PanelGameOver    → puntuación final + récord + botón "Reintentar" + botón "Menú"
//   └── PanelMuerte      → aviso breve "¡Perdiste una vida!" (dura 1.5 segundos)
//
public class GestorUI : MonoBehaviour
{
    // ---- PANEL DE MENÚ DE BIENVENIDA ----
    [Header("Menú de Bienvenida")]
    public GameObject      panelMenu;
    public TextMeshProUGUI textoRecordMenu;   // Muestra el récord en el menú ("Récord: 12")

    // ---- HUD (se ve mientras juegas) ----
    [Header("HUD del juego")]
    public GameObject        panelHUD;
    public TextMeshProUGUI   textoPuntuacion;   // Número de puntos en el centro arriba
    public TextMeshProUGUI   textoVidas;        // Corazones según las vidas que quedan
    public TextMeshProUGUI   textoNivel;        // "Nivel 1", "Nivel 2", etc.
    public TextMeshProUGUI   textoAvisoNivel;   // Aparece 2 segundos cuando subes de nivel

    // ---- PANTALLA DE GAME OVER ----
    [Header("Game Over")]
    public GameObject        panelGameOver;
    public TextMeshProUGUI   textoPuntuacionFinal;  // "Puntuación: 15"
    public TextMeshProUGUI   textoRecordGameOver;   // "Récord: 20"

    // ---- PANTALLA DE VIDA PERDIDA ----
    [Header("Pantalla de vida perdida (respawn)")]
    public GameObject        panelMuerte;
    public TextMeshProUGUI   textoVidasRestantes;   // "¡Cuidado! Te quedan 2 vidas"

    // ==============================================
    //  MOSTRAR / OCULTAR PANELES
    // ==============================================

    public void MostrarMenu(bool mostrar)
        => panelMenu.SetActive(mostrar);

    public void MostrarHUD(bool mostrar)
        => panelHUD.SetActive(mostrar);

    public void MostrarGameOver(bool mostrar)
        => panelGameOver.SetActive(mostrar);

    public void MostrarPantallaMuerte(bool mostrar)
    {
        panelMuerte.SetActive(mostrar);

        // Cuando aparece, mostramos cuántas vidas quedan
        if (mostrar && textoVidasRestantes != null)
        {
            int vidasQueQuedan = GameManager.Instance.ObtenerVidas();
            textoVidasRestantes.text = vidasQueQuedan == 1
                ? "¡Última vida! Ten cuidado"
                : $"¡Cuidado! Te quedan {vidasQueQuedan} vidas";
        }
    }

    // ==============================================
    //  ACTUALIZAR TEXTOS DEL HUD
    // ==============================================

    public void ActualizarPuntuacion(int puntos)
    {
        if (textoPuntuacion != null)
            textoPuntuacion.text = puntos.ToString();
    }

    public void ActualizarVidas(int vidas)
    {
        if (textoVidas == null) return;

        // Mostramos corazones según las vidas que quedan: ♥ ♥ ♥ → ♥ ♥ → ♥
        string corazones = new string('♥', Mathf.Max(vidas, 0));
        textoVidas.text  = corazones.Length > 0 ? corazones : "☆";
    }

    public void ActualizarNivel(int nivel)
    {
        if (textoNivel != null)
            textoNivel.text = "Nivel " + nivel;
    }

    public void ActualizarMejorPuntuacion(int record)
    {
        if (textoRecordMenu != null)
            textoRecordMenu.text = record > 0 ? "Récord: " + record : "";
    }

    // ==============================================
    //  AVISO DE SUBIDA DE NIVEL
    //  Aparece "¡NIVEL 2!" durante 2 segundos y desaparece solo
    // ==============================================

    public void AvisarSubidaNivel(int nivel)
    {
        if (textoAvisoNivel == null) return;

        textoAvisoNivel.text = "¡NIVEL " + nivel + "!";
        textoAvisoNivel.gameObject.SetActive(true);

        // Cancelamos el aviso anterior (por si el jugador subió dos niveles muy rápido)
        CancelInvoke(nameof(OcultarAvisoNivel));
        Invoke(nameof(OcultarAvisoNivel), 2f);
    }

    void OcultarAvisoNivel()
    {
        if (textoAvisoNivel != null)
            textoAvisoNivel.gameObject.SetActive(false);
    }

    // ==============================================
    //  PUNTUACIÓN FINAL EN GAME OVER
    // ==============================================

    public void MostrarPuntuacionFinal(int puntos, int record)
    {
        if (textoPuntuacionFinal != null)
            textoPuntuacionFinal.text = "Puntuación: " + puntos;

        if (textoRecordGameOver != null)
            textoRecordGameOver.text = "Récord: " + record;
    }

    // ==============================================
    //  BOTONES
    //  Cómo conectarlos en Unity:
    //  Selecciona el botón → Inspector → OnClick (+)
    //  → arrastra el GestorUI → elige el método
    // ==============================================

    // Botón "JUGAR" del menú de bienvenida
    public void AlPulsarJugar()
        => GameManager.Instance.IniciarPartida();

    // Botón "REINTENTAR" de la pantalla de game over
    public void AlPulsarReintentar()
        => GameManager.Instance.Reintentar();

    // Botón "MENÚ" de la pantalla de game over
    public void AlPulsarMenu()
        => GameManager.Instance.VolverAlMenu();
}
