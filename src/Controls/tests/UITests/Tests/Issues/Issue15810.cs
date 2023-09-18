using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue15810 : _IssuesUITest
	{
		readonly string _customViewId = "CustomView";

		public Issue15810(TestDevice device) : base(device)
		{
		}

		public override string Issue => "TapGestureRecognizer Tapped events not worked in Windows Platform";

		[Test]
		public void WhenTapCustomViewThenChangesColor()
		{
			App.WaitForElement(_customViewId);
			App.Tap(_customViewId);

			var infoText = App.Query("InfoLabel").First().Text;
			Assert.IsNotEmpty(infoText);
		}
	}
}