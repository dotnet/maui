using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31238 : _IssuesUITest
{
	public Issue31238(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "Setting CharacterSpacing property on buttons causing crashing on iOS";

	[Test]
	[Category(UITestCategories.Button)]
	public void SettingCharacterSpacingShouldNotCrash()
	{
		App.WaitForElement("MauiButton");
		App.Tap("MauiButton");
		App.Tap("MauiButton");
		App.Tap("MauiButton");
		App.WaitForElement("MauiButton");
	}
}