using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25368 : _IssuesUITest
	{
		public Issue25368(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Android RefreshView in a grid can break the grid layout";

		[Test]
		[Category(UITestCategories.RefreshView)]
		public void Issue2775Test()
		{
			App.WaitForElement("1");
			App.ScrollDown("CollectionView", ScrollStrategy.Gesture, swipePercentage: 0.9);
			App.ScrollDown("CollectionView", ScrollStrategy.Gesture, swipePercentage: 0.9);
			App.ScrollDown("CollectionView", ScrollStrategy.Gesture, swipePercentage: 0.9);
			App.WaitForElement("100");
		}
	}
}
