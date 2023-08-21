// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public partial class FontImageSourceService : ImageSourceService, IImageSourceService<IFontImageSource>
	{
		public FontImageSourceService(IFontManager fontManager)
			: this(fontManager, null)
		{
		}

		public FontImageSourceService(IFontManager fontManager, ILogger<FontImageSourceService>? logger = null)
			: base(logger)
		{
			FontManager = fontManager;
		}

		public IFontManager FontManager { get; }
	}
}