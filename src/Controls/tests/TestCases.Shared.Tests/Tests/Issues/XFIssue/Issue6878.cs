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
	const string TopTab = "Top Tab";
#if ANDROID
    const string PostClearTopTab = "POST CLEAR TOP TAB";
#else
	const string PostClearTopTab = "Post clear Top Tab";
#endif

	public override string Issue => "ShellItem.Items.Clear() crashes when the ShellItem has bottom tabs";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellItemItemsClearTests()
	{
		App.WaitForElement(StatusLabel);
		App.Tap(ClearShellItems);

		var label = App.WaitForElement(StatusLabel);
		Assert.That(label.ReadText(), Is.EqualTo(StatusLabelText));
		TapTobTab(PostClearTopTab);
	}
	void TapTobTab(string tab)
	{
		//In Windows the TopTab items are inside the root TabItem which shows in popup, so we need to tap it once to make them visible.
#if WINDOWS
        App.Tap("navViewItem");
#endif
		App.Tap(tab);
	}
}
