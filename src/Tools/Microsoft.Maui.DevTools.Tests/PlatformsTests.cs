// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.DevTools.Models;
using Xunit;

namespace Microsoft.Maui.DevTools.Tests;

public class PlatformsTests
{
	[Theory]
	[InlineData("android", true)]
	[InlineData("ios", true)]
	[InlineData("maccatalyst", true)]
	[InlineData("windows", true)]
	[InlineData("all", true)]
	[InlineData("ANDROID", true)]
	[InlineData("IOS", true)]
	[InlineData("invalid", false)]
	[InlineData("", false)]
	[InlineData(null, false)]
	public void IsValid_ReturnsCorrectResult(string? platform, bool expected)
	{
		Assert.Equal(expected, Platforms.IsValid(platform));
	}

	[Theory]
	[InlineData("android", "android")]
	[InlineData("ANDROID", "android")]
	[InlineData("ios", "ios")]
	[InlineData("apple", "ios")]
	[InlineData("iphone", "ios")]
	[InlineData("ipad", "ios")]
	[InlineData("mac", "maccatalyst")]
	[InlineData("macos", "maccatalyst")]
	[InlineData("catalyst", "maccatalyst")]
	[InlineData("maccatalyst", "maccatalyst")]
	[InlineData("windows", "windows")]
	[InlineData("win", "windows")]
	[InlineData("win32", "windows")]
	[InlineData("win64", "windows")]
	[InlineData(null, "all")]
	[InlineData("unknown", "unknown")]
	public void Normalize_ReturnsCorrectResult(string? input, string expected)
	{
		Assert.Equal(expected, Platforms.Normalize(input));
	}
}
