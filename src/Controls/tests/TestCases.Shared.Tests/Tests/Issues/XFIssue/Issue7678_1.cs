using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue7678_1 : _IssuesUITest
	{
		public override string Issue => "[Android] CarouselView binded to a new ObservableCollection filled with Items does not render content";
		public Issue7678_1(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void VerifyCarouselViewBindingAndRendering()
		{
			App.WaitForElement("carouselView", timeout: TimeSpan.FromSeconds(2));
			App.WaitForElementTillPageNavigationSettled("1");
		}
	}
}