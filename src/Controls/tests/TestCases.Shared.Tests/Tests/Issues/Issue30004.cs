using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue30004 : _IssuesUITest
{
	public override string Issue => "FontImageSource not center-aligned inside Image control";

	public Issue30004(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Image)]
	public void VerifyFontImageAreCenterAlign()
	{
		App.WaitForElement("FontImage");
		VerifyScreenshot();
	}
}