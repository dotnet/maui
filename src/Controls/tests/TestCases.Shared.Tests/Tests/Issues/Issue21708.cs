using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21708 : _IssuesUITest
{
	public Issue21708(TestDevice device) : base(device) { }

	public override string Issue => "CollectionView.Scrolled event offset isn't correctly reset when items change on Android";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyCollectionViewVerticalOffset()
	{
		App.WaitForElement("Fill");
		App.Tap("Fill");
		App.ScrollDown("CollectionView");
		App.Tap("Empty");
		App.Tap("Fill");
		App.ScrollDown("CollectionView");
		Assert.That(App.FindElement("Label").GetText(), Is.LessThanOrEqualTo("50"));
	}
}