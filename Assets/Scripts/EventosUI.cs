using System;
using UnityEngine;

public static class EventosUI
{
    /// <summary>Se invoca cuando la puntuación cambia. Parámetro: nueva puntuación</summary>
    public static event Action<int> OnPuntuacionCambiada;
    
    /// <summary>Se invoca cuando las vidas cambian. Parámetro: nuevas vidas restantes</summary>
    public static event Action<int> OnVidasCambiadas;
    
    /// <summary>Se invoca cuando cambia el nivel. Parámetro: nuevo nivel alcanzado</summary>
    public static event Action<int> OnNivelCambiado;

    public static event Action OnPuntoLogrado;

    public static void InvocarPuntuacionCambiada(int nuevaPuntuacion)
    {
        if (nuevaPuntuacion < 0)
        {
            Debug.LogWarning("[EventosUI] La puntuación enviada a la UI no puede ser negativa.");
            return;
        }
        OnPuntuacionCambiada?.Invoke(nuevaPuntuacion);
    }

    public static void InvocarVidasCambiadas(int nuevasVidas)
    {
        if (nuevasVidas < 0)
        {
            Debug.LogWarning("[EventosUI] Las vidas enviadas a la UI no pueden ser negativas.");
            return;
        }
        OnVidasCambiadas?.Invoke(nuevasVidas);
    }

    public static void InvocarPuntoLogrado()
    {
        OnPuntoLogrado?.Invoke();
    }

    public static void InvocarNivelCambiado(int nuevoNivel)
    {
        if (nuevoNivel < 1)
        {
            Debug.LogWarning("[EventosUI] El nivel enviado a la UI debe ser 1 o superior.");
            return;
        }
        OnNivelCambiado?.Invoke(nuevoNivel);
    }

    /// <summary>Limpia todos los listeners estáticos para evitar Memory Leaks entre escenas</summary>
    public static void LimpiarListeners()
    {
        OnPuntuacionCambiada = null;
        OnVidasCambiadas = null;
        OnNivelCambiado = null;
        OnPuntoLogrado = null;
    }
}