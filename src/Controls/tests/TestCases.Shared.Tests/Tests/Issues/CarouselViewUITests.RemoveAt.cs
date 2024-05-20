using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class CarouselViewRemoveAt : _IssuesUITest
	{
		public CarouselViewRemoveAt(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "ObservableCollection.RemoveAt(index) with a valid index raises ArgementOutOfRangeException";

		// Issue10300 (src\ControlGallery\src\Issues.Shared\Issue10300.cs
		[Test]
		[Category(UITestCategories.CarouselView)]
		[Ignore("Currently fails")]
		public void Issue10300Test()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS },
				"Currently fails on iOS; see https://github.com/dotnet/maui/issues/19488");

			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows },
				"iOS specific Test");

			App.Click("Add");
			App.Click("Delete");
			App.WaitForElement("Close");
			App.Click("Close");
			App.WaitForNoElement("2");
		}
	}
}