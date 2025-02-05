#if IOS || MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue10234 : _IssuesUITest
	{
		public override string Issue => "CarouselView disposed on iOS when navigating back in Shell";

		public Issue10234(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void ScrollCarouselViewAfterDispose()
		{
			App.WaitForElement("goToShow");
			App.Tap("goToShow");
			App.WaitForElement("goToBack");
			ScrollNextItem();
			App.Tap("goToBack");
			App.WaitForElement("goToShow");
			App.Tap("goToShow");
			ScrollNextItem();
			App.WaitForElement("goToBack");
			App.Tap("goToBack");
			App.WaitForElement("goToShow");
		}

		void ScrollNextItem()
		{
			App.ScrollRight("carouselView");
		}
	}
}
#endif