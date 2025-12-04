using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue13537 : _IssuesUITest
	{
#if ANDROID
		const string backButtonIdentifier = "";
#else
		const string backButtonIdentifier = "Home";
#endif
		public Issue13537(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ApplyQueryAttributes should Trigger for all navigations";

		[Test]
		[Category(UITestCategories.Shell)]
		public void ApplyQueryAttributeShouldTriggerforPushAndPopButton()
		{
			App.WaitForElement("HomePageTestLabel");
			App.Tap("PushAsyncButton");
			App.WaitForElement("InnerPageTestLabel");
			App.Tap("PopAsyncButton");
			var result = App.WaitForElement("HomePageTestLabel").GetText();
			Assert.That(result, Is.EqualTo("Issue13537HomePage QueryAttribute is triggered"));
		}

#if TEST_FAILS_ON_CATALYST
		//App.TapBackArrow doesnot actually tab the back arrow button instead it taps the first tab element on the screen as both have the same access identifier.
		[Test]
		[Category(UITestCategories.Shell)]
		public void ApplyQueryAttributeShouldTriggerforPushAndBackButton()
		{
			App.WaitForElement("HomePageTestLabel");
			App.Tap("PushAsyncButton");
			App.WaitForElement("InnerPageTestLabel");
#if IOS
			App.Back();
#else
			App.TapBackArrow(backButtonIdentifier);
#endif
			var result = App.WaitForElement("HomePageTestLabel").GetText();
			Assert.That(result, Is.EqualTo("Issue13537HomePage QueryAttribute is triggered"));
		}
#endif


		[Test]
		[Category(UITestCategories.Shell)]
		public void ApplyQueryAttributeShouldTriggerforTab()
		{
			App.WaitForElement("Favorite");
			App.Tap("Favorite");
			App.WaitForElement("FavouritePageTestLabel");
			App.Tap("GoToAsyncButton");
			var result = App.WaitForElement("InnerPageTestLabel").GetText();
			Assert.That(result, Is.EqualTo("Parameter From Favorite Page to New Page"));
		}
	}

}
