#nullable enable
using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public class FontManager : IFontManager
	{
		public double DefaultFontSize => -1;

		public FontManager(IFontRegistrar fontRegistrar, IServiceProvider? serviceProvider = null)
		{
		}
	}
}