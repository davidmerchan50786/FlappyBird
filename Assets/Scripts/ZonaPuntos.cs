using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ZonaPuntos : MonoBehaviour
{
    [SerializeField] private int puntosOtorgados = 1;
    
    // Protección contra el "doble-trigger" si el pájaro roza el borde
    private bool yaContabilizado = false;

    void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger)
        {
            Debug.LogWarning("[ZonaPuntos] El Collider2D debe tener 'Is Trigger' marcado.");
            col.isTrigger = true; // Lo corregimos automáticamente por seguridad
        }
    }

    void OnEnable()
    {
        // ¡CRÍTICO PARA EL OBJECT POOL! 
        // Como la tubería se recicla y vuelve a aparecer, tenemos que recargar la zona.
        yaContabilizado = false;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (yaContabilizado) return;

        if (col.CompareTag("Jugador"))
        {
            yaContabilizado = true;
            GameManager.Instance?.SumarPuntos(puntosOtorgados);
            
            // Le decimos al bus de eventos que suene la campana de puntos
            // (Añadiremos este evento en el siguiente paso)
            EventosUI.InvocarPuntoLogrado();
        }
    }
}