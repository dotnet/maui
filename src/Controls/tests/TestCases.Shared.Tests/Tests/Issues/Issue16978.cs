using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue16978 : _IssuesUITest
	{
		public override string Issue => "[Android]Editor controls don't raise Completed event consistently";

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

