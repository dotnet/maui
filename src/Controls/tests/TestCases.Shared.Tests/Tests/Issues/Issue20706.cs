using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue20706 : _IssuesUITest
	{
		public override string Issue => "Stepper doesn't change increment value when being bound to a double in MVVM context (Windows)";
		public Issue20706(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Stepper)]
		public void ChangeIncrementValue()
		{
			App.WaitForElement("incrementButton");
			App.IncreaseStepper("myStepper");
			App.Click("incrementButton");
			App.IncreaseStepper("myStepper");
			App.DecreaseStepper("myStepper");
			VerifyScreenshot();
		}
	}
}