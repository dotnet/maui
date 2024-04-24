using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue10234 : _IssuesUITest
	{
		public override string Issue => "CarouselView disposed on iOS when navigating back in Shell";

		public Issue10234(TestDevice device) : base(device)
		{
		}

		[Test]
		public void ScrollCarouselViewAfterDispose()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows });

			try
			{
				_ = App.WaitForElement("GoToTest");
				App.Click("GoToTest");
				App.WaitForElement("goToShow");
				App.Click("goToShow");
				App.WaitForElement("goToBack");
				ScrollNextItem();
				App.Click("goToBack");
				App.WaitForElement("goToShow");
				App.Click("goToShow");
				ScrollNextItem();
				App.WaitForElement("goToBack");
				App.Click("goToBack");
				App.WaitForElement("goToShow");
			}
			finally{
				Reset();
			}
		}

		void ScrollNextItem()
		{
			App.ScrollRight("carouselView");
		}
	}
}
