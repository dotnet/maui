using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Tests.Issues
{
	internal class Issue28153 : _IssuesUITest
	{
		public Issue28153(TestDevice device) : base(device)
		{
		}
		public override string Issue => "The border color of the RadioButton is visible in Windows only";
		[Test]
		[Category(UITestCategories.RadioButton)]
		public void RadioButtonBorderColorWithoutBorderWidth()
		{
			App.WaitForElement("RadioButton");
			VerifyScreenshot();
		}
	}
}
