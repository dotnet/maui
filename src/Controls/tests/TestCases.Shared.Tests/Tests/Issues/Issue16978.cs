using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue16978 : _IssuesUITest
	{
		public override string Issue => "[Android]PanGestureRecognizer is not updated for Frame";

		public Issue16978(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Frame)]
		[Category(UITestCategories.Gestures)]
		public void GestureNotRecogonizedForFrame()
		{
			// Is a Android issue; see https://github.com/dotnet/maui/issues/16978
			App.WaitForElement("FrameGesturePage");
			App.DragCoordinates(100, 100, 200, 200);
			VerifyScreenshot();
		}
	}
}

