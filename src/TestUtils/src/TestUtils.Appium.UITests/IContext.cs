// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Appium;

namespace TestUtils.Appium.UITests
{
	public interface IContext : IDisposable
	{
		UITestContext CreateUITestContext(TestConfig testConfig);
	}
}
