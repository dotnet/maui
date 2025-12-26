#if TEST_FAILS_ON_WINDOWS // When selecting a single item, the selection background is applied to all items.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20062 : _IssuesUITest
{
	public Issue20062(TestDevice device) : base(device) { }

	public override string Issue => "CollectionView - SelectedItem visual state manager not working";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewSelectionChangesVisualState()
	{
		App.WaitForElement("CollectionView");
		App.Tap("FirstItem");
		App.Tap("ThirdItem");
		VerifyScreenshot();
	}
}
#endif