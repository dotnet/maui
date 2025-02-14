using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27608 : _IssuesUITest
	{
		public Issue27608(TestDevice device) : base(device) { }

		public override string Issue => "Items shapes are sometimes rendered incorrectly using CollectionView2Handler";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewBorderItemsShouldRenderCorrectly()
		{
			App.WaitForElement("CollectionView");
			App.ScrollRight("CollectionView", swipePercentage: 0.99);
			Thread.Sleep(1000);
			VerifyScreenshot();
		}
	}
}