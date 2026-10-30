#nullable enable
using System;
using System.IO;

namespace Microsoft.Maui
{
	/// <inheritdoc/>
	public partial class FontRegistrar : IFontRegistrar
	{
		/// <summary>
		/// Supplies the content of a font registered by file name (<c>AddFont("Name.ttf")</c>) to an app host built on the platform-less
		/// assemblies (for example the Linux/GTK 4 backend, which installs it from <c>UseMauiGtkCore</c>). It returns the font stream or
		/// <see langword="null"/>; the stream is handed to the <see cref="IEmbeddedFontLoader"/> and disposed. With the default
		/// <see langword="null"/> provider there is no native font lookup: the font stays unresolved, as in the stock Standard behaviour.
		/// </summary>
		internal static Func<string, Stream?>? NativeFontStreamProvider { get; set; }

		string? LoadNativeAppFont(string font, string filename, string? alias)
		{
			if (NativeFontStreamProvider?.Invoke(filename) is not { } stream)
				return null;

			using (stream)
				return LoadEmbeddedFont(font, filename, alias, stream);
		}
	}
}