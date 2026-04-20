using UnityEngine;

// Cambia el sprite del fondo según la pantalla o el nivel en el que estés
// Ponlo en el mismo GameObject que el SpriteRenderer del fondo principal
public class GestorFondo : MonoBehaviour
{
    [Header("SpriteRenderer del fondo")]
    public SpriteRenderer rendererFondo;  // Arrastrarlo desde el Inspector

    [Header("Fondos de las pantallas")]
    public Sprite fondoMenu;             // Pantalla de bienvenida
    public Sprite fondoGameOver;         // Pantalla de game over

    [Header("Fondos por nivel (durante el juego)")]
    public Sprite fondoNivel1;           // Nivel 1 — por ejemplo: día
    public Sprite fondoNivel2;           // Nivel 2 — por ejemplo: tarde
    public Sprite fondoNivel3;           // Nivel 3 — por ejemplo: noche

    void Awake()
    {
        // Si olvidaste arrastrar el SpriteRenderer, lo buscamos en el mismo objeto
        if (rendererFondo == null)
            rendererFondo = GetComponent<SpriteRenderer>();
    }

    // ---- Métodos que llama el GameManager ----

    public void CambiarAMenu()
        => CambiarSprite(fondoMenu);

    public void CambiarANivel(int nivel)
    {
        Sprite sprite = nivel switch
        {
            1 => fondoNivel1,
            2 => fondoNivel2,
            3 => fondoNivel3,
            _ => fondoNivel1
        };
        CambiarSprite(sprite);
    }

    public void CambiarAGameOver()
        => CambiarSprite(fondoGameOver);

    // ---- Método interno ----

    void CambiarSprite(Sprite nuevoSprite)
    {
        if (nuevoSprite == null || rendererFondo == null) return;
        rendererFondo.sprite = nuevoSprite;
    }
}
