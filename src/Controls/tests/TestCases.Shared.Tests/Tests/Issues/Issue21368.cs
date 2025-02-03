#if !MACCATALYST
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
	public class Issue21368 : _IssuesUITest
	{
		public override string Issue => "Image AspectFill is not honored";

		public Issue21368(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Image)]
		public void VerifyImageAspects()
		{
			App.WaitForElement("Label");
			VerifyScreenshot();
		}
	}
}
#endif
