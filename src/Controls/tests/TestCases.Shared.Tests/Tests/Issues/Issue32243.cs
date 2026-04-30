using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.CollectionView)]
public class Issue32243 : _IssuesUITest
{
	public Issue32243(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "CollectionView does not disconnect handlers when DataTemplateSelector changes template";

	[Test]
	public void CollectionViewDisconnectsHandlersAfterNavigationBack()
	{
		App.WaitForElement("NavigateButton");
		App.Tap("NavigateButton");

		App.WaitForElement("SwitchTemplateButton");
		App.Tap("SwitchTemplateButton");

		App.WaitForElement("NavigateBackButton");
		App.Tap("NavigateBackButton");

		App.WaitForElement("CheckHandlersButton");
		App.Tap("CheckHandlersButton");

		var result = App.WaitForElement("HandlerCountLabel").GetText();
		Assert.That(result, Is.EqualTo("✓ No labels have connected handlers (all cleaned up!)"),
			"CollectionView should disconnect handlers from views belonging to the old DataTemplate after navigating back.");
	}
}
