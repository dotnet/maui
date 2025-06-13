using Xunit;
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

		public override string Issue => "ObservableCollection.RemoveAt(index) with a valid index raises ArgumentOutOfRangeException";

		[Fact]
		[Category(UITestCategories.CarouselView)]
		public void Issue10300Test()
		{
			App.WaitForElement("AddMe");
			App.Click("AddMe");
			App.WaitForElement("DeleteMe");
			App.Click("DeleteMe");
			App.WaitForElement("CloseMe");
			App.Click("CloseMe");
			App.WaitForElement("2");
		}
	}
}
