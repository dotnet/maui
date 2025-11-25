using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22183 : _IssuesUITest
	{
		public Issue22183(TestDevice device) : base(device)
		{
		}

		public override string Issue => "RadioButton with value cannot display selected state correctly";


		[Test]
		[Category(UITestCategories.RadioButton)]
		public void RadioButtonWithValueChangeSelected()
		{
			App.WaitForElement("TestCollectionView");

			App.Tap("True_0");
			App.Tap("False_0");

			VerifyScreenshot();

			App.Tap("True_1");
			App.Tap("False_1");
		}
	}
}