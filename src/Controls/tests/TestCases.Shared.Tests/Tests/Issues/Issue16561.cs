#if TEST_FAILS_ON_CATALYST //related issue: https://github.com/dotnet/maui/issues/17435
using System.Drawing;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using UITest.Appium;
using UITest.Core;
using PointerInputDevice = OpenQA.Selenium.Appium.Interactions.PointerInputDevice;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue16561 : _IssuesUITest
	{
		private string _tapAreaId = "TapArea";

		public Issue16561(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Quick single taps on Android have wrong second tap location";

		[Test]
		[Category(UITestCategories.Label)]
		[FailsOnMacWhenRunningOnXamarinUITest("https://github.com/dotnet/maui/issues/17435")]
		public void TapTwoPlacesQuickly()
		{
			if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
			{
				throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
			}

			var tapAreaResult = App.WaitForElement(_tapAreaId, $"Timed out waiting for {_tapAreaId}");
			var tapArea = tapAreaResult.GetRect();

			// The test harness coordinates are absolute
			var xOffset = 50;
			var harnessCenterX = tapArea.CenterX();
			var harnessCenterY = tapArea.CenterY();

			var point1 = new PointF(harnessCenterX - xOffset, harnessCenterY);
			var point2 = new PointF(harnessCenterX + xOffset, harnessCenterY);

			// The TapGesture coordinates are relative to the container, so we need to adjust
			// for the container position
			var expectedY = harnessCenterY - tapArea.Y;
			var expectedX1 = point1.X - tapArea.X;
			var expectedX2 = point2.X - tapArea.X;

			TapTwice(app2, point1, point2);

			// The results for each tap should show up in the labels on the screen; find the text
			// of each tap result and check to see that it meets the expected values
			var result = App.WaitForElement("Tap1Label", $"Timed out waiting for Tap1Label");
			AssertCorrectTapLocation(result.GetText()!, expectedX1, expectedY, "First");

			result = App.WaitForElement("Tap2Label", $"Timed out waiting for Tap2Label");
			AssertCorrectTapLocation(result.GetText()!, expectedX2, expectedY, "Second");
		}

		static void TapTwice(AppiumApp app, PointF point1, PointF point2)
		{
			var driver = app.Driver ?? throw new InvalidOperationException("The Appium driver is null; cannot perform taps.");

			app.TapCoordinates(point1.X, point1.Y);
			app.TapCoordinates(point2.X, point2.Y);
		}

		static void AssertCorrectTapLocation(string tapData, float expectedX, float expectedY, string which)
		{
			// Turn the text values into numbers so we can compare with a tolerance
			(var tapX, var tapY) = ParseCoordinates(tapData);

			ClassicAssert.AreEqual((double)expectedX, tapX, 1.5, $"{which} tap has unexpected X value");
			ClassicAssert.AreEqual((double)expectedY, tapY, 1.5, $"{which} tap has unexpected Y value");
		}

		static (double, double) ParseCoordinates(string text)
		{
			var values = text.Split(',', StringSplitOptions.TrimEntries)
				.Select(double.Parse)
				.ToArray();

			return (values[0], values[1]);
		}
	}
}
#endif