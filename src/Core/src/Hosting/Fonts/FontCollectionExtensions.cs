#nullable enable
using System;
using System.Reflection;

namespace Microsoft.Maui.Hosting
{
	public static class FontCollectionExtensions
	{
		/// <summary>
		/// Adds the font specified in <paramref name="filename"/> to the <paramref name="fontCollection"/>, with an optional font alias specified in <paramref name="alias"/>.
		/// </summary>
		/// <param name="fontCollection">The collection to add the font to.</param>
		/// <param name="filename">The filename of the font to add, such as a True type format (TTF) or open type font (OTF) font file. Font files can be added to the 'Resources\Fonts' of a .NET MAUI project.</param>
		/// <param name="alias">An optional alias that can also be used to reference this font.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">The <paramref name="filename"/> parameter is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">The <paramref name="filename"/> parameter is empty or only whitespace.</exception>
		public static IFontCollection AddFont(this IFontCollection fontCollection, string filename, string? alias = null)
		{
			_ = filename ?? throw new ArgumentNullException(nameof(filename));
			if (string.IsNullOrWhiteSpace(filename))
				throw new ArgumentException("Filename was not a valid file name.", nameof(filename));

			fontCollection.Add(new FontDescriptor(filename, alias, null));
			return fontCollection;
		}

		/// <summary>
		/// Adds the font specified in <paramref name="filename"/> from an embedded resource in <paramref name="assembly"/> to the <paramref name="fontCollection"/>, with an optional font alias specified in <paramref name="alias"/>.
		/// </summary>
		/// <param name="fontCollection">The collection to add the font to.</param>
		/// <param name="assembly">The assembly that contains the specified font as an embedded resource.</param>
		/// <param name="filename">The embedded resource filename of the font to add, such as a True type format (TTF) or open type font (OTF) font file.</param>
		/// <param name="alias">An optional alias that can also be used to reference this font.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">The <paramref name="filename"/> parameter is <c>null</c>. -or- The <paramref name="assembly"/> parameter is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">The <paramref name="filename"/> parameter is empty or only whitespace.</exception>
		public static IFontCollection AddEmbeddedResourceFont(this IFontCollection fontCollection, Assembly assembly, string filename, string? alias = null)
		{
			_ = assembly ?? throw new ArgumentNullException(nameof(assembly));
			_ = filename ?? throw new ArgumentNullException(nameof(filename));
			if (string.IsNullOrWhiteSpace(filename))
				throw new ArgumentException("Filename was not a valid file name.", nameof(filename));

			fontCollection.Add(new FontDescriptor(filename, alias, assembly));
			return fontCollection;
		}
	}
}