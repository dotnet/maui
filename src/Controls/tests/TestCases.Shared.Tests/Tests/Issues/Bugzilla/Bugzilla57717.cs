using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla57717 : _IssuesUITest
{
	const string ButtonText = "I am a button";

	public Bugzilla57717(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Setting background color on Button in Android FormsApplicationActivity causes NRE";

	[Test]
	[Category(UITestCategories.Button)]
	public void ButtonBackgroundColorAutomatedTest()
	{
		App.WaitForElement(ButtonText);
	}
}