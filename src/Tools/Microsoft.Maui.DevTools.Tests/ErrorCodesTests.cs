// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.DevTools.Errors;
using Xunit;

namespace Microsoft.Maui.DevTools.Tests;

public class ErrorCodesTests
{
	[Fact]
	public void ErrorCodes_HaveCorrectFormat()
	{
		// All error codes should start with 'E' followed by digits
		Assert.StartsWith("E", ErrorCodes.InternalError);
		Assert.StartsWith("E", ErrorCodes.JdkNotFound);
		Assert.StartsWith("E", ErrorCodes.AndroidSdkNotFound);
		Assert.StartsWith("E", ErrorCodes.XcodeNotFound);
	}

	[Fact]
	public void ErrorCodes_Categories_AreCorrectlyGrouped()
	{
		// Tool errors: E1xxx
		Assert.StartsWith("E1", ErrorCodes.InternalError);
		Assert.StartsWith("E1", ErrorCodes.DeviceNotFound);

		// JDK errors: E20xx
		Assert.StartsWith("E20", ErrorCodes.JdkNotFound);
		Assert.StartsWith("E20", ErrorCodes.JdkVersionUnsupported);

		// Android errors: E21xx
		Assert.StartsWith("E21", ErrorCodes.AndroidSdkNotFound);
		Assert.StartsWith("E21", ErrorCodes.AndroidSdkManagerNotFound);

		// Apple errors: E22xx
		Assert.StartsWith("E22", ErrorCodes.XcodeNotFound);
		Assert.StartsWith("E22", ErrorCodes.SimctlFailed);

		// User action: E3xxx
		Assert.StartsWith("E3", ErrorCodes.XcodeInstallRequired);
		Assert.StartsWith("E3", ErrorCodes.LicenseAcceptanceRequired);

		// Network errors: E4xxx
		Assert.StartsWith("E4", ErrorCodes.NetworkUnavailable);
		Assert.StartsWith("E4", ErrorCodes.DownloadFailed);

		// Permission errors: E5xxx
		Assert.StartsWith("E5", ErrorCodes.ElevationRequired);
		Assert.StartsWith("E5", ErrorCodes.AccessDenied);
	}
}
