#if ANDROID
using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue1931 : IssuesUITest
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
		public void ScrollViewInHeaderDisposesProperly()
		{
			RunningApp.WaitForElement(Go);
			RunningApp.Tap(Go);

			RunningApp.WaitForElement(Back);
			RunningApp.Tap(Back);

			RunningApp.WaitForNoElement(Success);
		}
	}
}
#endif