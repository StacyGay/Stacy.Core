using System;
using System.Drawing;

namespace Stacy.Core.Data
{
    public static class ColorExtensions
    {
        public static string GenerateRgbaFromHex(string hexBackgroundColor, double backgroundOpacity = 1)
        {
            var color = ColorTranslator.FromHtml(NormalizeHexColor(hexBackgroundColor));
            return $"rgba({Convert.ToInt16(color.R)}, {Convert.ToInt16(color.G)}, {Convert.ToInt16(color.B)}, {backgroundOpacity})";
        }

        public static int RgbTotalValueFromHex(string hexBackgroundColor)
        {
            var color = ColorTranslator.FromHtml(NormalizeHexColor(hexBackgroundColor));
            return Convert.ToInt16(color.R) + Convert.ToInt16(color.G) + Convert.ToInt16(color.B);
        }

        public static float HexDarknessPercentage(string hexBackgroundColor)
        {
            var color = ColorTranslator.FromHtml(NormalizeHexColor(hexBackgroundColor));
            return 1 - color.GetBrightness();
        }

        public static string GenerateContrastColorFromHex(string hexBackgroundColor)
        {
            var color = ColorTranslator.FromHtml(NormalizeHexColor(hexBackgroundColor));
            // Calculate the perceptive luminance (aka luma) - human eye favors green color... 
            double luma = ((0.299 * color.R) + (0.587 * color.G) + (0.114 * color.B)) / 255;

            // Return black for bright colors, white for dark colors
            return luma > 0.5 ? "#000000" : "#ffffff";
        }

        private static string NormalizeHexColor(string hexBackgroundColor)
        {
            if (hexBackgroundColor[0] != '#')
                hexBackgroundColor = "#" + hexBackgroundColor;

            return hexBackgroundColor;
        }
    }
}
