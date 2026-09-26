using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue7453 : _IssuesUITest
	{
		public Issue7453(TestDevice device) : base(device)
		{
		}

		public override string Issue => "ShellContent Title doesn't observe changes to bound properties";

		[Test]
		[Category(UITestCategories.Shell)]
		[FailsOnMacWhenRunningOnXamarinUITest("Flaky in CI (failed->passed on same SHA on net11.0; see ui-flake-quarantine-20260922.csv). Re-enable after flakiness investigation.")]
		public void ChangeShellContentTitle()
		{
			App.WaitForElement("ChangeShellContentTitle");
			App.Click("ChangeShellContentTitle");
			VerifyScreenshot();
		}
	}
}