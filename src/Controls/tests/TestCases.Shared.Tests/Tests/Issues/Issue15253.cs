using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Legacy;
using NUnit.Framework;
using UITest.Core;
using UITest.Appium;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue15253 : _IssuesUITest
	{
		public override string Issue => "[Windows]HorizontalScrollBarVisibility doesnot works";
		public Issue15253(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void HorizontalScrollBarShouldHideOnNever()
		{
			// Is a Windows issue; see https://github.com/dotnet/maui/issues/15253
			App.WaitForElement("15253CarouselView");
			VerifyScreenshot();
		}
	}
}
