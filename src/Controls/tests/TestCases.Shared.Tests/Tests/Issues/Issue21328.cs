using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue21328 : _IssuesUITest
    {
		public override string Issue => "Image has wrong orientation on iOS";

		public Issue21328(TestDevice device) : base(device)
		{
		}

		public void VerifyImageAspects()
		{
			App.WaitForElement("LabelNames");
			VerifyScreenshot();
		}
	}
}
