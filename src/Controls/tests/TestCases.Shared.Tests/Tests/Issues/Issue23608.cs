using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23608 : _IssuesUITest
	{
		public Issue23608(TestDevice device) : base(device)
		{
		}

		public override string Issue => "The checkbox's checked state color does not update when the IsEnabled property is changed dynamically";

		[Test]
		[Category(UITestCategories.CheckBox)]
		public void UpdatedIsEnabledProperty()
		{
			App.WaitForElement("Label");
			App.Click("Switch");
			VerifyScreenshot();
		}
	}
}