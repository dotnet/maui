using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31606 : _IssuesUITest
{
	public Issue31606(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "Crash on Windows when SemanticProperties.Description is set for ToolbarItem";

	[Test]
	[Category(UITestCategories.Accessibility)]
	public void SettingSemanticPropertiesDescriptionDoesNotCrash()
	{
		App.WaitForElement("TestStatus");
	}
}