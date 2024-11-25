using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue13537 : _IssuesUITest
		{
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
			Assert.That(result , Is.EqualTo("Issue13537HomePage QueryAttribute is triggered"));
		}

		[Test]
		[Category(UITestCategories.Shell)]
		public void ApplyQueryAttributeShouldTriggerforPushAndBackButton()
		{
			App.WaitForElement("HomePageTestLabel");
			App.Tap("PushAsyncButton");
			App.WaitForElement("InnerPageTestLabel");
			App.TapBackArrow("Home");
			var result = App.WaitForElement("HomePageTestLabel").GetText();
			Assert.That(result , Is.EqualTo("Issue13537HomePage QueryAttribute is triggered"));
		}

		[Test]
		[Category(UITestCategories.Shell)]
		public void ApplyQueryAttributeShouldTriggerforTab()
		{
			App.Tap("Favorite");
			App.WaitForElement("FavouritePageTestLabel");
			App.Tap("GoToAsyncButton");
			var result = App.WaitForElement("InnerPageTestLabel").GetText();
			Assert.That(result , Is.EqualTo("Parameter From Favorite Page to New Page"));
		}
	}

}
