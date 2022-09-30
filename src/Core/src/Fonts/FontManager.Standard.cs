#nullable enable
using System;

namespace Microsoft.Maui
{
	/// <inheritdoc/>
	public class FontManager : IFontManager
	{
		/// <inheritdoc/>
		public double DefaultFontSize => -1;

		/// <summary>
		/// Creates a new <see cref="EmbeddedFontLoader"/> instance.
		/// </summary>
		/// <param name="fontRegistrar">A <see cref="IFontRegistrar"/> instance to retrieve details from about registered fonts.</param>
		/// <param name="serviceProvider">The applications <see cref="IServiceProvider"/>.
		/// Typically this is provided through dependency injection.</param>
		public FontManager(IFontRegistrar fontRegistrar, IServiceProvider? serviceProvider = null)
		{
		}
	}
}