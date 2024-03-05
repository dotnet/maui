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
			App.WaitForElement(Go);
			App.Click(Go);

			App.WaitForElement(Back);
			App.Click(Back);

			App.WaitForElement(Success);
		}

		[Test]
		[Category(UITestCategories.ScrollView)]
		public void Test4186()
		{
			App.WaitForElement(Go);
			App.Click(Go);
			App.WaitForElement("Chicken");
			App.Click("Chicken");
			App.WaitForElement(Go);
			App.Click(Go);
			App.WaitForElement("Chicken");
			App.Click("Chicken");
			App.WaitForNoElement(Success);
		}
	}
}
