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
	//	RunningApp.WaitForElement(StatusLabel);
	//	RunningApp.Tap(ClearShellItems);

	//	var label = RunningApp.WaitForElement(StatusLabel)[0];
	//	Assert.AreEqual(StatusLabelText, label.ReadText());
	//	RunningApp.Tap(PostClearTopTab);
	//}
}