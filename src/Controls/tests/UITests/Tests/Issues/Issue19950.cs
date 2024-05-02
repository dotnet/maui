using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues;

public class Issue19950 : _IssuesUITest
{
	public override string Issue => "[Android] ToolbarItem shows incorrect image";

	public Issue19950(TestDevice device)
		: base(device)
	{ }

    [Test]
	public void ToolbarItemShouldShowCorrectImage()
	{
		try
		{
			_ = App.WaitForElement("GoToTest");
			App.Tap("GoToTest");

			App.WaitForElement("button");
			App.Click("button");
			App.WaitForElement("labelOnPage2");

			//The test passes if the gorcery icon is visible
			VerifyScreenshot();
		}
		finally
		{
			Reset();
		}
	}
}
