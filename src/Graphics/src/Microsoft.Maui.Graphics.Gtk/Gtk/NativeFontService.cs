using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.Graphics.Native.Gtk {

	public class NativeFontService : AbstractFontService {

		public static NativeFontService Instance = new NativeFontService();

		private static Pango.Context? _systemContext;

		public Pango.Context SystemContext => _systemContext ??= Gdk.PangoHelper.ContextGet();

		private Pango.FontDescription? _systemFontDescription;

		public Pango.FontDescription SystemFontDescription => _systemFontDescription ??= SystemContext.FontDescription;

		private string? _systemFontName;

		public string SystemFontName => _systemFontName ??= $"{SystemFontDescription.Family} {SystemFontDescription.GetSize()}";

		private string? _boldSystemFontName;

		public string BoldSystemFontName => _boldSystemFontName ??= $"{SystemFontDescription.Family} {SystemFontDescription.GetSize()} bold";

		private IFontFamily[]? _fontFamilies;

		public override IFontFamily[] GetFontFamilies()
			=> _fontFamilies ??= SystemContext.FontMap?.Families?.Select(fam => fam.ToFontFamily()).OrderBy(f => f.Name).ToArray() ?? Array.Empty<IFontFamily>();

		public IEnumerable<IFontStyle> GetFontStylesFor(IFontFamily family) {
			var fam = SystemContext.FontMap?.Families?.FirstOrDefault(f => f.Name == family.Name);

			if (fam == null) yield break;

			foreach (var s in fam.GetAvailableFontStyles()) {
				yield return s;
			}

		}

		private IEnumerable<(Pango.FontFamily family, Pango.FontDescription description)> GetAvailableFamilyFaces(Pango.FontFamily? family) {

			if (family == default) yield break;

			foreach (var face in family.Faces)
				yield return (family, face.Describe());

		}

	}

}
