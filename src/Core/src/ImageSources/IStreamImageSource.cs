// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui
{
	public interface IStreamImageSource : IImageSource
	{
		Task<Stream> GetStreamAsync(CancellationToken cancellationToken = default);
	}
}