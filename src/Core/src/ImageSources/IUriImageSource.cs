// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using System;

namespace Microsoft.Maui
{
	public interface IUriImageSource : IImageSource
	{
		Uri Uri { get; }

		TimeSpan CacheValidity { get; }

		bool CachingEnabled { get; }
	}
}