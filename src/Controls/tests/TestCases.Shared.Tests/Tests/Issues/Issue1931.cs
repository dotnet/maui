using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1931 : _IssuesUITest
	{
		const string Go = "Go";
		const string Back = "GoBack";
		const string Success = "Success";

		public Issue1931(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Xamarin Forms on Android: ScrollView on ListView header crashes app when closing page";

		[Test]
		[Category(UITestCategories.ScrollView)]
		[Category(UITestCategories.Compatibility)]
		public void ScrollViewInHeaderDisposesProperly()
		{
			App.WaitForElement(Go);
			App.Tap(Go);

			App.WaitForElement(Back);
			App.Tap(Back);

			App.WaitForElement(Success);
		}
	}
}