using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue28118 : _IssuesUITest
{
	public Issue28118(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "CollectionView - empty view issues";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void EmptyViewShouldScroll()
	{
		App.WaitForElement("CollectionView");
		App.ScrollDown("CollectionView", ScrollStrategy.Gesture, 0.5);
		App.WaitForElement("SuccessLabel");
	}
}