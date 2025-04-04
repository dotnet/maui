using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26644 : _IssuesUITest
	{
		public Issue26644(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Label with a fixed WidthRequest has wrong height";

		[Test]
		[Category(UITestCategories.Layout)]
		[Category(UITestCategories.Label)]
		public void LabelHeightShouldBeCorrect()
		{
			App.WaitForElement("Label1");
			var text1 = App.FindElement("Label1").GetText();
			var text2 = App.FindElement("Label2").GetText();
			
			Assert.That(text1, Is.EqualTo(text2));
		}
	}
}