using System.Collections.Generic;
using System.Linq;
using Microsoft.Graphics.Canvas.Text;

namespace Microsoft.Maui.Graphics.Win2D
{
	public class W2DFontService : AbstractFontService
	{
		public static W2DFontService Instance = new W2DFontService();

		private IFontFamily[] _fontFamilies;

		protected W2DFontService() : base("Arial", "Helvetica")
		{
		}

		public override IFontFamily[] GetFontFamilies()
		{
			return _fontFamilies ?? (_fontFamilies = Initialize());
		}

		public IFontFamily[] Initialize()
		{
			var familyList = new List<W2DFontFamily>();
			var families = new Dictionary<string, W2DFontFamily>();

			var fontSet = CanvasFontSet.GetSystemFontSet();
			var fonts = fontSet.Fonts;

			foreach (var fontFace in fonts)
			{
				if (fontFace.FamilyNames.TryGetValue("en-us", out var familyName))
				{
					if (!families.TryGetValue(familyName, out var family))
					{
						family = new W2DFontFamily(familyName);
						families[familyName] = family;
						familyList.Add(family);
					}

					var postScriptName = familyName;

					var localizedName = fontFace.GetInformationalStrings(CanvasFontInformation.PostscriptName);
					var firstKey = localizedName?.Keys.FirstOrDefault();
					if (firstKey != null)
					{
						postScriptName = localizedName[firstKey];
					}

					if (fontFace.FaceNames.TryGetValue("en-us", out var name))
					{
						var fullName = familyName;

						if (!("Regular".Equals(name) || "Plain".Equals(name) || "Normal".Equals(name)))
							fullName = string.Format("{0} {1}", familyName, name);

						var style = new W2DFontStyle(family, postScriptName, name, fullName, fontFace.Style, fontFace.Weight);
						family.AddStyle(style);
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
