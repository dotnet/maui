using System.Drawing;
using Microsoft.Maui.Appium;
using NUnit.Framework;
using OpenQA.Selenium.Appium.MultiTouch;
using TestUtils.Appium.UITests;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue16561 : _IssuesUITest
	{
		private string _tapAreaId = "TapArea";

		public Issue16561(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Quick single taps on Android have wrong second tap location";

		[Test]
		public void TapTwoPlacesQuickly()
		{
			if (App is not IApp2 app2 || app2 is null || app2.Driver is null)
			{
				throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
			}

			var tapAreaResult = App.WaitForElement(_tapAreaId);
			var tapArea = tapAreaResult[0].Rect;

			// The test harness coordinates are absolute
			var xOffset = 50;
			var harnessCenterX = tapArea.CenterX;
			var harnessCenterY = tapArea.CenterY;

			var point1 = new PointF(harnessCenterX - xOffset, harnessCenterY);
			var point2 = new PointF(harnessCenterX + xOffset, harnessCenterY);

			// The TapGesture coordinates are relative to the container, so we need to adjust
			// for the container position
			var expectedY = harnessCenterY - tapArea.Y;
			var expectedX1 = point1.X - tapArea.X;
			var expectedX2 = point2.X - tapArea.X;

			// Just calling Tap twice will be too slow; we need to queue up the actions so they happen quickly
			var actionsList = new TouchAction(app2.Driver);

			// Tap the first point, then the second point
			actionsList.Tap(point1.X, point1.Y).Tap(point2.X, point2.Y);
			app2.Driver.PerformTouchAction(actionsList);

			// The results for each tap should show up in the labels on the screen; find the text
			// of each tap result and check to see that it meets the expected values
			var result = App.WaitForElement("Tap1Label");
			AssertCorrectTapLocation(result[0].Text, expectedX1, expectedY, "First");

			result = App.WaitForElement("Tap2Label");
			AssertCorrectTapLocation(result[0].Text, expectedX2, expectedY, "Second");
		}

		static void AssertCorrectTapLocation(string tapData, float expectedX, float expectedY, string which) 
		{
			// Turn the text values into numbers so we can compare with a tolerance
			(var tapX, var tapY) = ParseCoordinates(tapData);

			Assert.AreEqual((double)expectedX, tapX, 1, $"{which} tap has unexpected X value");
			Assert.AreEqual((double)expectedY, tapY, 1, $"{which} tap has unexpected Y value");
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
