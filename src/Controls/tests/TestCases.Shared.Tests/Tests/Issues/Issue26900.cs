using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26900 : _IssuesUITest
	{
		public Issue26900(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Scroll view doesn't scroll when its height is explicitly set";

		[Test]
		[Category(UITestCategories.ScrollView)]
		public void ScrollViewShouldScroll()
		{
			App.WaitForElement("label1");
			App.ScrollDown("label1", ScrollStrategy.Gesture, 0.90, 1000);
			App.WaitForElement("label2");
		}
	}
}