#nullable enable

using System;

namespace Microsoft.Maui
{
	public partial class EmbeddedFontLoader
	{
		public EmbeddedFontLoader(IServiceProvider? serviceProvider = null)
		{
			_serviceProvider = serviceProvider;
		}

		public string? LoadFont(EmbeddedFont font) => null;
	}
}