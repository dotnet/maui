using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19657 : _IssuesUITest
	{
		public Issue19657(TestDevice device) : base(device) { }

		public override string Issue => "CarouselView Content disappears when 'Loop' is false and inside ScrollView";

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void CarouselItemLoadsInCorrectPosition()
		{
			// The bug only happens on iOS; see https://github.com/dotnet/maui/issues/19657

			_ = App.WaitForElement("WaitHere");

			var element = App.WaitForElement("First");

			VerifyScreenshot();
		}
	}
}