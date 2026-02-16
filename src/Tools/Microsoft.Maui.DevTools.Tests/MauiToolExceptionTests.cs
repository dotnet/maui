// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.DevTools.Errors;
using Xunit;

namespace Microsoft.Maui.DevTools.Tests;

public class MauiToolExceptionTests
{
	[Fact]
	public void Constructor_WithCodeAndMessage_SetsProperties()
	{
		var ex = new MauiToolException(ErrorCodes.JdkNotFound, "JDK not installed");

		Assert.Equal(ErrorCodes.JdkNotFound, ex.Code);
		Assert.Equal("JDK not installed", ex.Message);
		Assert.Null(ex.Remediation);
		Assert.Null(ex.NativeError);
	}

	[Fact]
	public void Constructor_WithRemediation_SetsRemediationInfo()
	{
		var remediation = new RemediationInfo(
			Type: RemediationType.AutoFixable,
			Command: "maui android jdk install"
		);

		var ex = new MauiToolException(
			ErrorCodes.JdkNotFound,
			"JDK not installed",
			remediation: remediation);

		Assert.NotNull(ex.Remediation);
		Assert.Equal(RemediationType.AutoFixable, ex.Remediation.Type);
		Assert.Equal("maui android jdk install", ex.Remediation.Command);
	}

	[Fact]
	public void Constructor_WithNativeError_SetsNativeError()
	{
		var ex = new MauiToolException(
			ErrorCodes.AndroidSdkManagerNotFound,
			"sdkmanager not found",
			nativeError: "Command 'sdkmanager' not found in PATH");

		Assert.Equal("Command 'sdkmanager' not found in PATH", ex.NativeError);
	}

	[Fact]
	public void Constructor_WithContext_SetsContext()
	{
		var context = new Dictionary<string, object>
		{
			["sdk_path"] = "/path/to/sdk",
			["expected_version"] = 35
		};

		var ex = new MauiToolException(
			ErrorCodes.AndroidPackageNotFound,
			"Package not found",
			context: context);

		Assert.NotNull(ex.Context);
		Assert.Equal("/path/to/sdk", ex.Context["sdk_path"]);
		Assert.Equal(35, ex.Context["expected_version"]);
	}

	[Fact]
	public void AutoFixable_CreatesExceptionWithAutoFixableRemediation()
	{
		var ex = MauiToolException.AutoFixable(
			ErrorCodes.JdkNotFound,
			"JDK not found",
			"maui android jdk install");

		Assert.Equal(ErrorCodes.JdkNotFound, ex.Code);
		Assert.NotNull(ex.Remediation);
		Assert.Equal(RemediationType.AutoFixable, ex.Remediation.Type);
		Assert.Equal("maui android jdk install", ex.Remediation.Command);
	}

	[Fact]
	public void UserActionRequired_CreatesExceptionWithManualSteps()
	{
		var steps = new[] { "Open App Store", "Search for Xcode", "Install" };
		var ex = MauiToolException.UserActionRequired(
			ErrorCodes.XcodeNotFound,
			"Xcode not found",
			steps);

		Assert.NotNull(ex.Remediation);
		Assert.Equal(RemediationType.UserAction, ex.Remediation.Type);
		Assert.Equal(steps, ex.Remediation.ManualSteps);
	}

	[Fact]
	public void Terminal_CreatesNonFixableException()
	{
		var ex = MauiToolException.Terminal(
			ErrorCodes.PlatformNotSupported,
			"Platform not supported");

		Assert.NotNull(ex.Remediation);
		Assert.Equal(RemediationType.Terminal, ex.Remediation.Type);
	}
}
