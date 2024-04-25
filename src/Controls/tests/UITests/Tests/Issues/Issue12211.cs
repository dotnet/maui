using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue12211 : _IssuesUITest
	{
		private string buttonId = "ChangeOpacity";

		public Issue12211(TestDevice device) : base(device)
		{
		}

		public override string Issue => "[Android] BoxView Opacity not working";

		[Test]
		[Category(UITestCategories.BoxView)]
		public void WhenChangingBoxViewOpacityThenValueIsCorrectlySet()
		{
			App.WaitForElement(buttonId);

			var initialOpacity = GetCurrentOpacityStatus();
			var secondOpacity = ChangeOpacityAndGetCurrentStatus();
			var thirdOpacity = ChangeOpacityAndGetCurrentStatus();

			Assert.AreEqual(GetExpectedCurrentOpacityStatus(0.7), initialOpacity);
			Assert.AreEqual(GetExpectedCurrentOpacityStatus(1), secondOpacity);
			Assert.AreEqual(GetExpectedCurrentOpacityStatus(0), thirdOpacity);
		}

		string? ChangeOpacityAndGetCurrentStatus()
		{
			App.Tap(buttonId);

			return GetCurrentOpacityStatus();
		}

		string? GetCurrentOpacityStatus() => App.FindElement("CurrentOpacity").GetText();

		string GetExpectedCurrentOpacityStatus(double expectedOpacity) => $"Current opacity is {expectedOpacity}";
	}
}
