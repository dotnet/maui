using System;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Gtk.Platform;

namespace Microsoft.Maui.Gtk.Fonts
{
	/// <summary>
	/// <see cref="IFontManager"/> that resolves MAUI <see cref="Font"/> values to Pango font descriptions and CSS font declarations.
	/// </summary>
	public sealed class GtkFontManager : IFontManager
	{
		const int PangoScale = 1024;

		readonly IFontRegistrar? _fontRegistrar;
		double _defaultFontSize;

		/// <summary>Creates the manager.</summary>
		/// <param name="fontRegistrar">Resolves registered font aliases to files; may be <see langword="null"/> when no aliases are used.</param>
		/// <param name="serviceProvider">The application services (unused, accepted for parity with the stock manager).</param>
		public GtkFontManager(IFontRegistrar? fontRegistrar, IServiceProvider? serviceProvider = null)
		{
			_fontRegistrar = fontRegistrar;
		}

		/// <summary>The default font size in device-independent pixels, taken from the <c>gtk-font-name</c> setting (14 when unavailable).</summary>
		public double DefaultFontSize
		{
			get
			{
				if (_defaultFontSize > 0)
					return _defaultFontSize;

				double size = 14;
				try
				{
					GtkRuntime.EnsureInitialized();
					var settings = global::Gtk.Settings.Default;
					var fontName = settings?.FontName;
					if (!string.IsNullOrEmpty(fontName))
					{
						var description = Pango.FontDescription.FromString(fontName);
						double dpi = settings!.XftDpi > 0 ? settings.XftDpi / 1024.0 : 96.0;
						if (description.Size > 0)
							size = Math.Round(description.Size / (double)PangoScale * dpi / 72.0);
					}
				}
				catch (Exception)
				{
					// keep the fallback
				}

				return _defaultFontSize = size;
			}
		}

		/// <summary>
		/// Resolves the family to hand to Pango/CSS: the family name inside the file registered for the alias
		/// (<see cref="IFontRegistrar.GetFont"/>), or the requested name itself.
		/// </summary>
		/// <param name="family">The family or registered alias; may be empty.</param>
		/// <returns>The Pango family name, or <see langword="null"/> for the theme default.</returns>
		public string? GetFontFamily(string? family)
		{
			if (string.IsNullOrWhiteSpace(family))
				return null;

			var path = _fontRegistrar?.GetFont(family!);
			if (path is not null)
			{
				var fileFamily = FontConfigHelper.QueryFamilyName(path);
				if (!string.IsNullOrEmpty(fileFamily))
					return fileFamily;
			}

			return family;
		}

		/// <summary>Builds the Pango font description for <paramref name="font"/>; the size is absolute (device pixels).</summary>
		/// <param name="font">The MAUI font.</param>
		public Pango.FontDescription GetFontDescription(Font font)
		{
			var description = new Pango.FontDescription();

			var family = GetFontFamily(font.Family);
			if (family is not null)
				description.Family = family;

			description.Weight = ToPangoWeight((int)font.Weight);
			description.Style = font.Slant switch
			{
				FontSlant.Italic => Pango.Style.Italic,
				FontSlant.Oblique => Pango.Style.Oblique,
				_ => Pango.Style.Normal,
			};

			var size = font.Size > 0 ? font.Size : DefaultFontSize;
			description.AbsoluteSize = size * PangoScale;

			return description;
		}

		/// <summary>Builds the CSS font declarations (<c>font-family</c>, <c>font-size</c>, <c>font-weight</c>, <c>font-style</c>) for <paramref name="font"/>.</summary>
		/// <param name="font">The MAUI font.</param>
		public string GetCssFont(Font font)
		{
			var css = new StringBuilder();

			var family = GetFontFamily(font.Family);
			if (family is not null)
				css.Append("font-family: \"").Append(family.Replace("\"", "\\\"", StringComparison.Ordinal)).Append("\"; ");

			var size = font.Size > 0 ? font.Size : DefaultFontSize;
			css.Append(string.Format(CultureInfo.InvariantCulture, "font-size: {0:0.##}px; ", size));
			css.Append("font-weight: ").Append(((int)font.Weight).ToString(CultureInfo.InvariantCulture)).Append("; ");
			css.Append("font-style: ").Append(font.Slant switch
			{
				FontSlant.Italic => "italic",
				FontSlant.Oblique => "oblique",
				_ => "normal",
			}).Append(';');

			return css.ToString();
		}

		static Pango.Weight ToPangoWeight(double weight) =>
			(Pango.Weight)(int)Math.Max(100, Math.Min(1000, weight <= 0 ? 400 : weight));
	}
}
