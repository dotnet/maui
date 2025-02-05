using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3509 : _IssuesUITest
{
	const string _popPage = "Pop Page";

	public Issue3509(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] NavigationPage.Popped called twice when Navigation.PopAsync is called";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void PoppedOnlyFiresOnce()
	{
		App.WaitForElement(_popPage);
		App.Tap(_popPage);
		App.WaitForElement("1");

		App.Back();
	}
}