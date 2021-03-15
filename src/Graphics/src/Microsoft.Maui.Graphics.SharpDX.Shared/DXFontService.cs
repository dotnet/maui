using System.Collections.Generic;
using System.Linq;
using SharpDX.DirectWrite;

namespace Microsoft.Maui.Graphics.SharpDX
{
    public class DXFontService : AbstractFontService
    {
        public static DXFontService Instance = new DXFontService();

        private IFontFamily[] _fontFamilies;

        public override IFontFamily[] GetFontFamilies()
        {
            if (_fontFamilies == null)
                _fontFamilies = Initialize();

            return _fontFamilies;
        }

        public IFontFamily[] Initialize()
        {
            var familyList = new List<DXFontFamily>();
            var families = new Dictionary<string, DXFontFamily>();

            var fontCollection = DXGraphicsService.FactoryDirectWrite.GetSystemFontCollection(false);
            int familyCount = fontCollection.FontFamilyCount;

            for (int i = 0; i < familyCount; i++)
            {
                var fontFamily = fontCollection.GetFontFamily(i);
                var familyNames = fontFamily.FamilyNames;

                if (!familyNames.FindLocaleName("en-us", out var index))
                    index = 0;
                var familyName = familyNames.GetString(index);

                for (int j = 0; j < fontFamily.FontCount; j++)
                {
                    string postScriptName = familyName;

                    var font = fontFamily.GetFont(j);

                    var found = font.GetInformationalStrings(InformationalStringId.PostscriptName,
                        out var localizedPostScriptName);
                    if (found)
                    {
                        postScriptName = localizedPostScriptName.GetString(0);
                    }

                    if (!families.TryGetValue(familyName, out var family))
                    {
                        family = new DXFontFamily(familyName);
                        families[familyName] = family;
                        familyList.Add(family);
                    }

                    var id = postScriptName;
                    if (!font.FaceNames.FindLocaleName("en-us", out index))
                        index = 0;

                    var name = font.FaceNames.GetString(index);

                    var fullName = familyName;
                    if (!("Regular".Equals(name) || "Plain".Equals(name) || "Normal".Equals(name)))
                        fullName = string.Format("{0} {1}", familyName, name);

                    var fontFace = new FontFace(font);
                    var style = new DXFontStyle(family, id, name, fullName, font.Style, font.Weight, fontFace);
                    family.AddStyle(style);
                }
            }

            familyList.Sort();

            foreach (var family in familyList)
                family.RemoveDuplicates();

            return familyList.OfType<IFontFamily>().ToArray();
        }
    }
}