// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Application)]
	public class ApplicationTests
	{
		[Fact(DisplayName = "Verify Application.Current exists on IPlatformApplication")]
		public void PlatformApplicationCurrentExists()
		{
			var platform = Microsoft.Maui.IPlatformApplication.Current;
			Assert.NotNull(platform);
			Assert.NotNull(platform.Services);
			Assert.NotNull(platform.Application);
		}
	}
}
