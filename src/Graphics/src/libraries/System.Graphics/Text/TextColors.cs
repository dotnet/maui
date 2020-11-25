using System.Collections.Generic;
using System.Globalization;

namespace System.Graphics.Text
{
    public static class TextColors
    {
        public static Dictionary<string, string> StandardColors = new Dictionary<string, string>
        {
            {"BLACK", "#000000"},
            {"NAVY", "#000080"},
            {"DARKBLUE", "#00008B"},
            {"MEDIUMBLUE", "#0000CD"},
            {"BLUE", "#0000FF"},
            {"DARKGREEN", "#006400"},
            {"GREEN", "#008000"},
            {"TEAL", "#008080"},
            {"DARKCYAN", "#008B8B"},
            {"DEEPSKYBLUE", "#00BFFF"},
            {"DARKTURQUOISE", "#00CED1"},
            {"MEDIUMSPRINGGREEN", "#00FA9A"},
            {"LIME", "#00FF00"},
            {"SPRINGGREEN", "#00FF7F"},
            {"AQUA", "#00FFFF"},
            {"CYAN", "#00FFFF"},
            {"MIDNIGHTBLUE", "#191970"},
            {"DODGERBLUE", "#1E90FF"},
            {"LIGHTSEAGREEN", "#20B2AA"},
            {"FORESTGREEN", "#228B22"},
            {"SEAGREEN", "#2E8B57"},
            {"DARKSLATEGREY", "#2F4F4F"},
            {"LIMEGREEN", "#32CD32"},
            {"MEDIUMSEAGREEN", "#3CB371"},
            {"TURQUOISE", "#40E0D0"},
            {"ROYALBLUE", "#4169E1"},
            {"STEELBLUE", "#4682B4"},
            {"DARKSLATEBLUE", "#483D8B"},
            {"MEDIUMTURQUOISE", "#48D1CC"},
            {"INDIGO", "#4B0082"},
            {"DARKOLIVEGREEN", "#556B2F"},
            {"CADETBLUE", "#5F9EA0"},
            {"CORNFLOWERBLUE", "#6495ED"},
            {"MEDIUMAQUAMARINE", "#66CDAA"},
            {"DIMGREY", "#696969"},
            {"SLATEBLUE", "#6A5ACD"},
            {"OLIVEDRAB", "#6B8E23"},
            {"SLATEGREY", "#708090"},
            {"LIGHTSLATEGREY", "#778899"},
            {"MEDIUMSLATEBLUE", "#7B68EE"},
            {"LAWNGREEN", "#7CFC00"},
            {"CHARTREUSE", "#7FFF00"},
            {"AQUAMARINE", "#7FFFD4"},
            {"MAROON", "#800000"},
            {"PURPLE", "#800080"},
            {"OLIVE", "#808000"},
            {"GREY", "#808080"},
            {"SKYBLUE", "#87CEEB"},
            {"LIGHTSKYBLUE", "#87CEFA"},
            {"BLUEVIOLET", "#8A2BE2"},
            {"DARKRED", "#8B0000"},
            {"DARKMAGENTA", "#8B008B"},
            {"SADDLEBROWN", "#8B4513"},
            {"DARKSEAGREEN", "#8FBC8F"},
            {"LIGHTGREEN", "#90EE90"},
            {"MEDIUMPURPLE", "#9370D8"},
            {"DARKVIOLET", "#9400D3"},
            {"PALEGREEN", "#98FB98"},
            {"DARKORCHID", "#9932CC"},
            {"YELLOWGREEN", "#9ACD32"},
            {"SIENNA", "#A0522D"},
            {"BROWN", "#A52A2A"},
            {"DARKGREY", "#A9A9A9"},
            {"LIGHTBLUE", "#ADD8E6"},
            {"GREENYELLOW", "#ADFF2F"},
            {"PALETURQUOISE", "#AFEEEE"},
            {"LIGHTSTEELBLUE", "#B0C4DE"},
            {"POWDERBLUE", "#B0E0E6"},
            {"FIREBRICK", "#B22222"},
            {"DARKGOLDENROD", "#B8860B"},
            {"MEDIUMORCHID", "#BA55D3"},
            {"ROSYBROWN", "#BC8F8F"},
            {"DARKKHAKI", "#BDB76B"},
            {"SILVER", "#C0C0C0"},
            {"MEDIUMVIOLETRED", "#C71585"},
            {"INDIANRED", "#CD5C5C"},
            {"PERU", "#CD853F"},
            {"CHOCOLATE", "#D2691E"},
            {"TAN", "#D2B48C"},
            {"LIGHTGREY", "#D3D3D3"},
            {"PALEVIOLETRED", "#D87093"},
            {"THISTLE", "#D8BFD8"},
            {"ORCHID", "#DA70D6"},
            {"GOLDENROD", "#DAA520"},
            {"CRIMSON", "#DC143C"},
            {"GAINSBORO", "#DCDCDC"},
            {"PLUM", "#DDA0DD"},
            {"BURLYWOOD", "#DEB887"},
            {"LIGHTCYAN", "#E0FFFF"},
            {"LAVENDER", "#E6E6FA"},
            {"DARKSALMON", "#E9967A"},
            {"VIOLET", "#EE82EE"},
            {"PALEGOLDENROD", "#EEE8AA"},
            {"LIGHTCORAL", "#F08080"},
            {"KHAKI", "#F0E68C"},
            {"ALICEBLUE", "#F0F8FF"},
            {"HONEYDEW", "#F0FFF0"},
            {"AZURE", "#F0FFFF"},
            {"SANDYBROWN", "#F4A460"},
            {"WHEAT", "#F5DEB3"},
            {"BEIGE", "#F5F5DC"},
            {"WHITESMOKE", "#F5F5F5"},
            {"MINTCREAM", "#F5FFFA"},
            {"GHOSTWHITE", "#F8F8FF"},
            {"SALMON", "#FA8072"},
            {"ANTIQUEWHITE", "#FAEBD7"},
            {"LINEN", "#FAF0E6"},
            {"LIGHTGOLDENRODYELLOW", "#FAFAD2"},
            {"OLDLACE", "#FDF5E6"},
            {"RED", "#FF0000"},
            {"FUCHSIA", "#FF00FF"},
            {"MAGENTA", "#FF00FF"},
            {"DEEPPINK", "#FF1493"},
            {"ORANGERED", "#FF4500"},
            {"TOMATO", "#FF6347"},
            {"HOTPINK", "#FF69B4"},
            {"CORAL", "#FF7F50"},
            {"DARKORANGE", "#FF8C00"},
            {"LIGHTSALMON", "#FFA07A"},
            {"ORANGE", "#FFA500"},
            {"LIGHTPINK", "#FFB6C1"},
            {"PINK", "#FFC0CB"},
            {"GOLD", "#FFD700"},
            {"PEACHPUFF", "#FFDAB9"},
            {"NAVAJOWHITE", "#FFDEAD"},
            {"MOCCASIN", "#FFE4B5"},
            {"BISQUE", "#FFE4C4"},
            {"MISTYROSE", "#FFE4E1"},
            {"BLANCHEDALMOND", "#FFEBCD"},
            {"PAPAYAWHIP", "#FFEFD5"},
            {"LAVENDERBLUSH", "#FFF0F5"},
            {"SEASHELL", "#FFF5EE"},
            {"CORNSILK", "#FFF8DC"},
            {"LEMONCHIFFON", "#FFFACD"},
            {"FLORALWHITE", "#FFFAF0"},
            {"SNOW", "#FFFAFA"},
            {"YELLOW", "#FFFF00"},
            {"LIGHTYELLOW", "#FFFFE0"},
            {"IVORY", "#FFFFF0"},
            {"WHITE", "#FFFFFF"}
        };

        public static float[] Parse(this string color)
        {
            if (string.IsNullOrEmpty(color))
                return null;

            //Remove # if present
            if (!color.StartsWith("#", StringComparison.Ordinal))
            {
                if (!StandardColors.TryGetValue(color.ToUpper(), out color))
                    return null;
            }

            int red = 0;
            int green = 0;
            int blue = 0;
            int alpha = 255;

            if (color.Length == 7)
            {
                //#RRGGBB
                red = int.Parse(color.Substring(1, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                green = int.Parse(color.Substring(3, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                blue = int.Parse(color.Substring(5, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
            }
            else if (color.Length == 4)
            {
                //#RGB
                red = int.Parse($"{color[1]}{color[1]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                green = int.Parse($"{color[2]}{color[2]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                blue = int.Parse($"{color[3]}{color[3]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
            }
            else if (color.Length == 9)
            {
                //#RRGGBBAA
                red = int.Parse(color.Substring(1, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                green = int.Parse(color.Substring(3, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                blue = int.Parse(color.Substring(5, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                alpha = int.Parse(color.Substring(7, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
            }

            return new[] {red / 255f, green / 255f, blue / 255f, alpha / 255f};
        }

        public static int[] ParseAsInts(this string color)
        {
            if (string.IsNullOrEmpty(color))
                return null;

            //Remove # if present
            if (!color.StartsWith("#", StringComparison.Ordinal))
            {
                if (!StandardColors.TryGetValue(color.ToUpper(), out color))
                    return null;
            }

            int red = 0;
            int green = 0;
            int blue = 0;
            int alpha = 255;

            if (color.Length == 7)
            {
                //#RRGGBB
                red = int.Parse(color.Substring(1, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                green = int.Parse(color.Substring(3, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                blue = int.Parse(color.Substring(5, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
            }
            else if (color.Length == 4)
            {
                //#RGB
                red = int.Parse($"{color[1]}{color[1]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                green = int.Parse($"{color[2]}{color[2]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                blue = int.Parse($"{color[3]}{color[3]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
            }
            else if (color.Length == 9)
            {
                //#RRGGBBAA
                red = int.Parse(color.Substring(1, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                green = int.Parse(color.Substring(3, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                blue = int.Parse(color.Substring(5, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                alpha = int.Parse(color.Substring(7, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
            }

            return new[] {red, green, blue, alpha};
        }
    }
}