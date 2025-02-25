using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue6878 : _IssuesUITest
{
	public Issue6878(TestDevice testDevice) : base(testDevice)
	{
	}
	const string ClearShellItems = "ClearShellItems";
	const string StatusLabel = "StatusLabel";
	const string StatusLabelText = "Everything is fine 😎";
	const string PostClearTopTab = "Post clear Top Tab";


	public override string Issue => "ShellItem.Items.Clear() crashes when the ShellItem has bottom tabs";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellItemItemsClearTests()
	{
		App.WaitForElement(ClearShellItems);
		App.Tap(ClearShellItems);
		var label = App.WaitForElement(StatusLabel);
		Assert.That(label.ReadText(), Is.EqualTo(StatusLabelText));
		App.TapTab(PostClearTopTab, true);
	}
}
