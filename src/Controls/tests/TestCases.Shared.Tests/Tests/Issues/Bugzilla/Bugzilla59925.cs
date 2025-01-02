using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla59925 : _IssuesUITest
	{
		public Bugzilla59925(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Font size does not change vertical height of Entry on iOS";

		[Test]
		[Category(UITestCategories.Entry)]
		[Category(UITestCategories.Compatibility)]
		public void Bugzilla59925Test()
		{
			App.WaitForElement("BiggerButton");
			var intialSize = App.WaitForElement("TestEntry").GetRect().Height;

#if IOS || MACCATALYST
			for (int i = 0; i < 8; i++)
			{
				App.WaitForElement("BiggerButton");
				App.Tap("BiggerButton");
			}
#endif

			App.Tap("BiggerButton");
			var updatedSize = App.WaitForElement("TestEntry").GetRect().Height;
			Assert.That(updatedSize, Is.GreaterThan(intialSize));

			App.Tap("BiggerButton");
			var updatedSize1 = App.WaitForElement("TestEntry").GetRect().Height;
			Assert.That(updatedSize1, Is.GreaterThan(updatedSize));
			
			App.Tap("BiggerButton");
			var updatedSize2 = App.WaitForElement("TestEntry").GetRect().Height;
			Assert.That(updatedSize2, Is.GreaterThan(updatedSize1));
		}
	}
}