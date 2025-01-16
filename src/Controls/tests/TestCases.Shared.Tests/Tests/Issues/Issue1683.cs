using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1683 : _IssuesUITest
	{
		public Issue1683(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Auto Capitalization Implementation";

		[Test]
		[Category(UITestCategories.Entry)]
		[Category(UITestCategories.Compatibility)]
		public void Issue1683Test()
		{
			App.WaitForElement("Rotation");

			for (int i = 0; i < 6; i++)
			{
				App.Tap("Rotation");
			}
		}
	}
}