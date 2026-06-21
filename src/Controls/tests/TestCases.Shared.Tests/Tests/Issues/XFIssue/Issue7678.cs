using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue7678 : _IssuesUITest
	{
		public override string Issue => "[iOS] CarouselView binded to a ObservableCollection and add Items later, crash";
		public Issue7678(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void VerifyCarouselViewItemsRenderAfterBinding()
		{
			App.WaitForElement("carouselView", timeout: TimeSpan.FromSeconds(4));
			App.WaitForElementTillPageNavigationSettled("1");
		}
	}
}