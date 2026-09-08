#if TEST_FAILS_ON_WINDOWS // Text is not rendered on windows. Issue -  https://github.com/dotnet/maui/issues/22731
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2927 : _IssuesUITest
{
	public Issue2927(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ListView item tapped not firing multiple times";

	[Test]
	[Category(UITestCategories.ListView)]
	public void Issue2927Test()
	{

		App.WaitForElement("Cell1 0");
		App.Tap("Cell1 0");
		App.WaitForElement("Cell1 1");

		App.Tap("Cell1 1");
		App.WaitForElement("Cell1 2");

		App.Tap("Cell3 0");
		App.WaitForElement("Cell3 1");

		App.Tap("Cell1 2");
		App.WaitForElement("Cell1 3");
	}
}
#endif