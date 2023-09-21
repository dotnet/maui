using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue5191 : _IssuesUITest
	{
		public Issue5191(TestDevice device)
		: base(device)
		{ }

		public override string Issue => "PanGesture notify Completed event moving outside View limits";

		[Test]
		public void PanGestureCompleted()
		{
			App.WaitForElement("PanGrid");

			App.DragCoordinates(0, 0, 0, 600);

			var statusText = App.Query("InfoLabel").First().Text;

			Assert.True(statusText.Contains("Completed", StringComparison.InvariantCultureIgnoreCase));
		}
	}
}
