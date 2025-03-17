using System.Globalization;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
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

			ClassicAssert.AreEqual(GetExpectedCurrentOpacityStatus(0.7), initialOpacity);
			ClassicAssert.AreEqual(GetExpectedCurrentOpacityStatus(1), secondOpacity);
			ClassicAssert.AreEqual(GetExpectedCurrentOpacityStatus(0), thirdOpacity);
		}

		string? ChangeOpacityAndGetCurrentStatus()
		{
			App.Tap(buttonId);

			return GetCurrentOpacityStatus();
		}

		string? GetCurrentOpacityStatus() => App.FindElement("CurrentOpacity").GetText();

		string GetExpectedCurrentOpacityStatus(double expectedOpacity) => $"Current opacity is {expectedOpacity.ToString(CultureInfo.InvariantCulture)}";
	}
}
