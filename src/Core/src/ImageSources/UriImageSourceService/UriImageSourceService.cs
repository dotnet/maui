// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public partial class UriImageSourceService : ImageSourceService, IImageSourceService<IUriImageSource>
	{
		public UriImageSourceService()
			: this(null)
		{
		}

		public UriImageSourceService(ILogger<UriImageSourceService>? logger = null)
			: base(logger)
		{
		}
	}
}