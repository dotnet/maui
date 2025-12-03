using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26633 : _IssuesUITest
	{
		public Issue26633(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Label height in Grid with ColumnSpacing > 0 incorrect in certain cases";

		[Test]
		[Category(UITestCategories.Layout)]
		[Category(UITestCategories.Label)]
		public void VerifyLabelHeightInGridWithColumnSpacing()
		{
			App.WaitForElement("Label1");

			var text1 = App.FindElement("Label1").GetText();
			var text2 = App.FindElement("Label2").GetText();

			Assert.That(text1, Is.EqualTo(text2));
		}
	}
}
