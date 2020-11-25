using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;

namespace System.Graphics.GDI
{
    public class GDIFontService : AbstractFontService
    {
        public static GDIFontService Instance = new GDIFontService();

        private IFontFamily[] _fontFamilies;

        public override IFontFamily[] GetFontFamilies()
        {
            return _fontFamilies ?? (_fontFamilies = Initialize());
        }

        public IFontFamily[] Initialize()
        {
            var styles = new[] {FontStyle.Bold, FontStyle.Italic, FontStyle.Regular};

            var familyList = new List<GDIFontFamily>();
            var families = new Dictionary<string, GDIFontFamily>();

            var fontCollection = new InstalledFontCollection();
            int familyCount = fontCollection.Families.Length;

            for (int i = 0; i < familyCount; i++)
            {
                var fontFamily = fontCollection.Families[i];
                var familyName = fontFamily.GetName(0);

                if (!families.TryGetValue(familyName, out var family))
                {
                    family = new GDIFontFamily(familyName);
                    families[familyName] = family;
                    familyList.Add(family);
                }

                foreach (var style in styles)
                {
                    if (fontFamily.IsStyleAvailable(style))
                    {
                        int weight = 200;
                        var id = familyName;
                        var name = familyName;

                        if (style == FontStyle.Bold)
                        {
                            weight = 700;
                            id += "-bold";
                            name += " Bold";
                        }
                        else if (style == FontStyle.Italic)
                        {
                            id += "-italic";
                            name += " Italic";
                        }

                        var fontStyle = new GDIFontStyle(family, id, name, name, style, weight);
                        family.AddStyle(fontStyle);
                    }
                }
            }

            familyList.Sort();

            foreach (var family in familyList)
                family.RemoveDuplicates();

            return familyList.OfType<IFontFamily>().ToArray();
        }
    }
}