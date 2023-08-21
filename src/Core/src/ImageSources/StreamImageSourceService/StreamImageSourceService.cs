// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public partial class StreamImageSourceService : ImageSourceService, IImageSourceService<IStreamImageSource>
	{
		public StreamImageSourceService()
			: this(null)
		{
		}

		public StreamImageSourceService(ILogger<StreamImageSourceService>? logger = null)
			: base(logger)
		{
		}
	}
}