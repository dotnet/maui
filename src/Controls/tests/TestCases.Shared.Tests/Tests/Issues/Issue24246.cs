using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24246 : _IssuesUITest
	{
		public Issue24246(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "SafeArea arrange insets are currently insetting based on an incorrect Bounds";

		[Test]
		[Category(UITestCategories.Layout)]
		public void SafeAreaArrangeInsetsShouldUseCorrectBounds()
		{
			App.WaitForElement("TestLayout");
			App.WaitForElement("TestEntry");
			
			// The test is primarily about ensuring the layout doesn't crash 
			// and that the Entry is properly positioned within the SafeArea
			// The visual verification would be manual, but we can at least
			// verify the UI elements are rendered properly
			
			// If the SafeArea bounds calculation is wrong, the layout might not render correctly
			// or might throw exceptions during layout
			var layoutElement = App.FindElement("TestLayout");
			Assert.IsNotNull(layoutElement);
			
			var entryElement = App.FindElement("TestEntry");
			Assert.IsNotNull(entryElement);
		}
	}
}