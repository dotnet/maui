using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue17022 : _IssuesUITest
	{
		public Issue17022(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "UINavigationBar is Translucent";

		[Test]
		public void Issue17022Test()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows }, "This test is only for iOS");

            var boxView = App.WaitForElement("TopBoxView");
            Assert.NotNull(boxView);
			var rect = boxView.GetRect();
            Assert.AreEqual(rect.Y, 0);
		}
	}
}
