// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Appium;
using Xamarin.UITest;

namespace TestUtils.Appium.UITests
{
	public interface IUITestContext : IDisposable
	{
		public IApp App { get; }
		public TestConfig TestConfig { get; }
	}
}
