using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue13537 : _IssuesUITest
		{
		public Issue13537(TestDevice testDevice) : base(testDevice)
		{
		}

			public override string Issue => "ApplyQueryAttributes should Trigger for all navigations";

		[Test]
		[Category(UITestCategories.Shell)]
		public void ApplyQueryAttributeShouldTriggerforPushAndPopButton()
		{
			App.WaitForElement("TestLabel");
			App.Tap("PushAsyncButton");
			App.WaitForElement("TestLabel3");
			App.Tap("PopAsyncButton");
			VerifyScreenshot();
		}
	}

}
