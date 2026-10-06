using UnityEngine;

// Colores del juego. Si agregas uno nuevo aquí, la rueda se reparte sola.
public enum ColorJuego
{
    Azul,
    Verde,
    Morado,
    Naranja
}

public static class ColorJuegoUtil
{
    public static int Cantidad => System.Enum.GetValues(typeof(ColorJuego)).Length;

    // Color que se usa en la UI de la rueda (ajústalo al tono de tus materiales)
    public static Color AColor(ColorJuego c) => c switch
    {
        ColorJuego.Azul    => new Color(0.20f, 0.45f, 0.95f),
        ColorJuego.Verde   => new Color(0.25f, 0.80f, 0.35f),
        ColorJuego.Morado  => new Color(0.60f, 0.30f, 0.85f),
        ColorJuego.Naranja => new Color(1.00f, 0.55f, 0.15f),
        _ => Color.white
    };
}
