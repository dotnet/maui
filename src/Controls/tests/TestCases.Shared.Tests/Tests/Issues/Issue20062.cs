#if TEST_FAILS_ON_WINDOWS // When select an single item, the selection background is applied to all items.
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
#if ANDROID
		App.TapCoordinates(80, 180);
		App.TapCoordinates(80, 450);
#elif IOS
App.TapCoordinates(50, 100);
		App.TapCoordinates(50, 280);
#elif MACCATALYST
var firstItem = App.FindElement("a").GetRect();
		var X = firstItem.X + 5;
		App.TapCoordinates(X, 150);
		App.TapCoordinates(X, 250);
#endif
		VerifyScreenshot();
	}
}
#endif