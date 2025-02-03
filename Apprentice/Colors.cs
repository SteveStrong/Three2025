using System.Drawing;

public class TemperatureColors
{
    public static Color[] GetTemperatureColors()
    {
        Color[] colors = new Color[121];
        for (int i = 0; i <= 120; i++)
        {
            int temperature = i - 20;
            colors[i] = GetColorForTemperature(temperature);
        }
        return colors;
    }

    private static Color GetColorForTemperature(int temperature)
    {
        // Example gradient from blue (-20°C) to red (100°C)
        int r = (int)((temperature + 20) * 255 / 120);
        int g = 0;
        int b = 255 - r;
        return Color.FromArgb(r, g, b);
    }
}