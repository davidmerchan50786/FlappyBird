using UnityEngine;

public class GestorFondo : MonoBehaviour
{
    [Header("Backgrounds por nivel")]
    public Sprite fondoNivel1;  
    public Sprite fondoNivel2;  
    public Sprite fondoNivel3;  

    [Header("Referencias")]
    public SpriteRenderer rendererFondo;
    public Transform fondoCopia;
    public Camera camaraJuego;

    void Awake()
    {
        // Fallback de seguridad: si olvidas asignar la cámara, busca la principal
        if (camaraJuego == null) camaraJuego = Camera.main;

        // Ejecutamos todo en Awake para que la imagen ya esté escalada 
        // antes de que el ScrollFondo empiece a medirla en su Start.
        AsignarFondo();
        AjustarTamaño();
    }

    void AsignarFondo()
    {
        // CORRECCIÓN: Le preguntamos al GestorNivel, no al GameManager
        int nivel = GestorNivel.Instance != null ? GestorNivel.Instance.ObtenerNivelActual() : 1;

        if (rendererFondo != null)
        {
            rendererFondo.sprite = nivel switch
            {
                1 => fondoNivel1,
                2 => fondoNivel2,
                3 => fondoNivel3,
                _ => fondoNivel1
            };
        }
    }

    void AjustarTamaño()
    {
        if (camaraJuego == null || rendererFondo == null || rendererFondo.sprite == null) return;

        float altoCamara = camaraJuego.orthographicSize * 2f;
        float anchoCamara = altoCamara * camaraJuego.aspect;

        float altoSprite = rendererFondo.sprite.bounds.size.y;
        float anchoSprite = rendererFondo.sprite.bounds.size.x;

        float escalaX = anchoCamara / anchoSprite;
        float escalaY = altoCamara / altoSprite;

        float escala = Mathf.Max(escalaX, escalaY);
        
        // Aplicamos la escala al fondo original
        rendererFondo.transform.localScale = new Vector3(escala, escala, 1f);

        // Solo le aplicamos la escala a la copia. 
        // Dejamos que el script ScrollFondo se encargue de posicionarla.
        if (fondoCopia != null)
        {
            fondoCopia.localScale = new Vector3(escala, escala, 1f);
        }
    }
}