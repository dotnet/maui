#nullable enable
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public class FontManager : IFontManager
	{
		public FontManager(IFontRegistrar fontRegistrar, ILogger<FontManager>? logger = null)
		{
		}
	}
}