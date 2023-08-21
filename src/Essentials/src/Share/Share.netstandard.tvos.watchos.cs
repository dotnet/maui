// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel.DataTransfer
{
	partial class ShareImplementation : IShare
	{
		Task PlatformRequestAsync(ShareTextRequest request) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		Task PlatformRequestAsync(ShareFileRequest request) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		Task PlatformRequestAsync(ShareMultipleFilesRequest request) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
