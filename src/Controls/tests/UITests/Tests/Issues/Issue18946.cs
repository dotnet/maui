using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18946 : _IssuesUITest
	{
		public override string Issue => "Shell Toolbar items not displayed";

		public Issue18946(TestDevice device) : base(device)
		{ }

		[Test]
		public void ToolbarItemsShouldBeVisible()
		{
			var label = App.WaitForElement("label");
			_ = App.WaitForElement("image");

			Assert.True(label.GetText() == "Hello");
		}
	}
}
