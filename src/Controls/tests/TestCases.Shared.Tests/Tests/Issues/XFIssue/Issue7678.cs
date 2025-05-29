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
			var text  = App.WaitForElement("1").GetText();
			Assert.That(text, Is.EqualTo("1"), "The first item in the Carouselview should be `1` after binding.");
		}
	}
}