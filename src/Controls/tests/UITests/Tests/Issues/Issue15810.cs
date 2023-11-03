using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

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
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.iOS, TestDevice.Mac });

			App.WaitForElement(_customViewId);
			App.Click(_customViewId);

			var infoText = App.FindElement("InfoLabel").GetText();
			Assert.IsNotEmpty(infoText);
		}
	}
}