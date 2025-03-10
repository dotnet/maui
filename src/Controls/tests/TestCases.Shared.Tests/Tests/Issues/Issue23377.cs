using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue23377 : _IssuesUITest
{
	public override string Issue => "Item Spacing misbehaviour for horizontal list";
	public Issue23377(TestDevice device) : base(device){ }

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void Issue23377ItemSpacing()
	{
		App.WaitForElement("EntryControl");
		App.EnterText("EntryControl", "100");
		App.Tap("ChangeItemSpace");
		App.WaitForElement("ChangeItemSpace");
		VerifyScreenshot();
	}
}

