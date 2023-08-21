// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class RefreshViewTests : _IssuesUITest
	{
		public RefreshViewTests(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Refresh View Tests";

		[Test]
		public void IsRefreshingAndCommandTest()
		{
			App.Tap("ToggleRefresh");
			Assert.IsTrue(App.WaitForTextToBePresentInElement("IsRefreshingLabel", "IsRefreshing: True"));
			Assert.IsTrue(App.WaitForTextToBePresentInElement("IsRefreshingLabel", "IsRefreshing: False"));
		}
	}
}
