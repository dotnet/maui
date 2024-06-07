
using System.Drawing;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22914 : _IssuesUITest
	{
		const string _buttonId = "Tap1Button";
		private string[] _expectedNullBackgroundColorIds = [_buttonId, "ContentView1", "Label1", "VerticalStackLayout1"];
		public Issue22914(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Setting BackgroundColor to null does not actually changes BackgroundColor";

		[Test]
		[Category(UITestCategories.ViewBaseTests)]
		public void TapTwoPlacesQuickly()
		{
			if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
			{
				throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
			}

			var tapAreaResult = App.WaitForElement(_buttonId, $"Timed out waiting for {_buttonId}");
			tapAreaResult.Tap();

			foreach (var elementId in _expectedNullBackgroundColorIds)
			{
				var element = App.WaitForElement(elementId, $"Timed out waiting for {elementId}");
				var elementBackgroundColor = element.GetAttribute<Color?>("BackgroundColor");
				ClassicAssert.IsTrue(elementBackgroundColor is null, $"{elementId} has unexpected BackgroundColor");
			}
		}
	}
}
