// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.ApplicationModel;
using Xunit;

namespace Tests
{
	public class Utils_Tests
	{
		[Theory]
		[InlineData("9", "9.0", 9, 0)]
		[InlineData("9.0", "9.0", 9, 0)]
		public void ParseVersion(string input, string expected, int major, int minor)
		{
			var version = Utils.ParseVersion(input);
			Assert.Equal(expected, version.ToString());
			Assert.Equal(major, version.Major);
			Assert.Equal(minor, version.Minor);
		}
	}
}
