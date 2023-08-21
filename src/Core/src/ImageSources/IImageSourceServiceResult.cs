// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using System;

namespace Microsoft.Maui
{
	public interface IImageSourceServiceResult<T> : IImageSourceServiceResult
	{
		T Value { get; }
	}

	public interface IImageSourceServiceResult : IDisposable
	{
		bool IsResolutionDependent { get; }

		bool IsDisposed { get; }
	}
}