// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Client.Utils;
using Xunit;

namespace Microsoft.Maui.Client.Tests;

public class ProcessRunnerTests
{
	[Fact]
	public void SanitizeArg_NullInput_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => ProcessRunner.SanitizeArg(null!));
	}

	[Theory]
	[InlineData(";")]
	[InlineData("&")]
	[InlineData("|")]
	[InlineData("`")]
	[InlineData("$")]
	[InlineData("\n")]
	[InlineData("\r")]
	[InlineData("\0")]
	public void SanitizeArg_ForbiddenCharacter_ThrowsArgumentException(string forbidden)
	{
		var ex = Assert.Throws<ArgumentException>(() => ProcessRunner.SanitizeArg($"test{forbidden}value"));
		Assert.Contains("forbidden characters", ex.Message);
	}

	[Fact]
	public void SanitizeArg_CommandInjection_ThrowsArgumentException()
	{
		Assert.Throws<ArgumentException>(() => ProcessRunner.SanitizeArg("test; rm -rf /"));
		Assert.Throws<ArgumentException>(() => ProcessRunner.SanitizeArg("test && evil"));
		Assert.Throws<ArgumentException>(() => ProcessRunner.SanitizeArg("test | cat /etc/passwd"));
		Assert.Throws<ArgumentException>(() => ProcessRunner.SanitizeArg("$(whoami)"));
		Assert.Throws<ArgumentException>(() => ProcessRunner.SanitizeArg("`whoami`"));
	}

	[Fact]
	public void SanitizeArg_ValueWithSpaces_ReturnsQuoted()
	{
		var result = ProcessRunner.SanitizeArg("hello world");
		Assert.Equal("\"hello world\"", result);
	}

	[Fact]
	public void SanitizeArg_SimpleValue_ReturnsUnchanged()
	{
		Assert.Equal("emulator-5554", ProcessRunner.SanitizeArg("emulator-5554"));
		Assert.Equal("ActivityManager:I", ProcessRunner.SanitizeArg("ActivityManager:I"));
		Assert.Equal("test123", ProcessRunner.SanitizeArg("test123"));
	}

	[Fact]
	public void SanitizeArg_EmptyString_ReturnsEmpty()
	{
		Assert.Equal("", ProcessRunner.SanitizeArg(""));
	}
}
