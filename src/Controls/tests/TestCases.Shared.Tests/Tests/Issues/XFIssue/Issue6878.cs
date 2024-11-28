using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue6878 : _IssuesUITest
{
	public Issue6878(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ShellItem.Items.Clear() crashes when the ShellItem has bottom tabs";

	//[Test]
	//[Category(UITestCategories.Shell)]
	//public void ShellItemItemsClearTests()
	//{
	//	App.WaitForElement(StatusLabel);
	//	App.Tap(ClearShellItems);

	//	var label = App.WaitForElement(StatusLabel)[0];
	//	Assert.AreEqual(StatusLabelText, label.ReadText());
	//	App.Tap(PostClearTopTab);
	//}
}