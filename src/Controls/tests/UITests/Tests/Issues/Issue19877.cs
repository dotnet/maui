using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue19877 : _IssuesUITest
	{
		public Issue19877(TestDevice device) : base(device) { }

		public override string Issue => "RoundRectangle Border is messed up when contains an Image with AspectFill";

		[Test]
		[Category(UITestCategories.Entry)]
		public void BorderRoundRectangleWithImage()
		{
			this.IgnoreIfPlatforms(new[]
			{
				TestDevice.iOS,
				TestDevice.Mac,
				TestDevice.Windows
			});

			App.WaitForElement("TestBorder");

			VerifyScreenshot();
		}
	}
}