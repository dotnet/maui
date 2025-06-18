#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // Issue reproduced and logged the issue report: https://github.com/dotnet/maui/issues/26091.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2954 : _IssuesUITest
{
	public Issue2954(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Cell becomes empty after adding a new one with context actions (TableView) ";

	[Test]
	[Category(UITestCategories.TableView)]
	public void Issue2954Test()
	{
		App.WaitForElement("Cell2");
		App.Tap("Add new");
		App.WaitForElement("Cell2");
	}
}
#endif