#if ANDROID // This Issue is only reprodcued in Android platform
using NUnit.Framework;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue13356 : _IssuesUITest
{

	public Issue13356(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Java.Lang.IllegalArgumentException: The style on this component requires your app theme to be Theme.MaterialComponent.";

	[Test]
	[Category(UITestCategories.Button)]
	public void MaterialButtonShouldHaveMaterialThemeComponent()
	{
		App.waitForElement("showButton");
		App.Tap("showButton");
		App.WaitForElement("Dialog Button");
	}
}
#endif