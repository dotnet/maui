using System;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29882 : _IssuesUITest
{
	public Issue29882(TestDevice device) : base(device)
	{
	}

	public override string Issue => "[iOS] Crash occurs when ItemsSource is set to null in the SelectionChanged handler";

	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void SettingItemSourceToNullShouldNotCrash()
	{
		App.WaitForElement("Item1");
		App.Tap("Item1");
		App.WaitForElement("MauiLabel"); // If app doesn't crash, test passes.
	}
}
