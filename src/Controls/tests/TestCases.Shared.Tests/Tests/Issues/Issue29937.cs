using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29937 : _IssuesUITest
{
	public Issue29937(TestDevice device) : base(device)
	{
	}

	public override string Issue => "[iOS/MacCatalyst] Setting SelectedItem Programmatically and Then Immediately Setting ItemsSource to Null Causes a Crash";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void SettingSelectedItemAndItemSourceShouldNotCrash()
	{
		App.WaitForElement("MauiButton");
		App.Tap("MauiButton");
		App.WaitForElement("MauiButton"); // If app doesn't crash, test passes.
	}
}
