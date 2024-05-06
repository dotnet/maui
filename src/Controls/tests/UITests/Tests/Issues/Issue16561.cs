using System.Drawing;
using NUnit.Framework;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using UITest.Appium;
using UITest.Core;
using PointerInputDevice = OpenQA.Selenium.Appium.Interactions.PointerInputDevice;

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
		[Category(UITestCategories.Label)]
		public void TapTwoPlacesQuickly()
		{
			// https://github.com/dotnet/maui/issues/17435
			this.IgnoreIfPlatforms(new[]
			{
				TestDevice.Mac
			});

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

			if (driver is WindowsDriver)
			{
				// Windows will throw an error if we try to execute Taps with a TouchAction
				// or if we try to use ExecuteScript, so we'll just use TapCoordinates instead
				app.Click(point1.X, point1.Y);
				app.Click(point2.X, point2.Y);
			}
			else if (driver is IOSDriver)
			{
				// iOS, on the other hand, will allow us to use ExecuteScript to run two taps quickly
				// It will not work with an ActionSequence, though; one of the taps will simply never 
				// happen. No errors, but no second tap.

				driver.ExecuteScript("mobile: tap", new Dictionary<string, object> {
					{ "x", point1.X },
					{ "y", point1.Y }
				});

				driver.ExecuteScript("mobile: tap", new Dictionary<string, object> {
					{ "x", point2.X },
					{ "y", point2.Y }
				});
			}
			else
			{
				// For Android, TapCoordinates won't work (it's far too slow), and ExecuteScript 
				// throws an error. So we'll use an ActionSequence, which is what we wanted in 
				// the first place.

				PointerInputDevice touchDevice = new PointerInputDevice(PointerKind.Touch);
				var sequence = new ActionSequence(touchDevice, 0);

				// Move to the first location and tap
				sequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport,
					(int)point1.X, (int)point1.Y, TimeSpan.FromMilliseconds(250)));
				sequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
				sequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));

				// If we don't put some kind of pause between the taps, Appium will throw an exception
				// We'll use a pause that's shorter than the default time for a double-tap on Android,
				// so we're very clearly simulating two different taps on two different locations
				sequence.AddAction(touchDevice.CreatePause(TimeSpan.FromMilliseconds(250)));

				// Move to the second location and tap
				sequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport,
					(int)point2.X, (int)point2.Y, TimeSpan.FromMilliseconds(0)));
				sequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
				sequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));

				// Run the sequence we just built
				driver.PerformActions(new List<ActionSequence> { sequence });
			}
		}

		static void AssertCorrectTapLocation(string tapData, float expectedX, float expectedY, string which)
		{
			// Turn the text values into numbers so we can compare with a tolerance
			(var tapX, var tapY) = ParseCoordinates(tapData);

			Assert.AreEqual((double)expectedX, tapX, 1.5, $"{which} tap has unexpected X value");
			Assert.AreEqual((double)expectedY, tapY, 1.5, $"{which} tap has unexpected Y value");
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
