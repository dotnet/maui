#if !MACCATALYST
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Legacy;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue8244 : _IssuesUITest
	{
		public override string Issue => "[iOS]Custom font icon is not rendered in a shell tabbar tab";

		public Issue8244(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Shell)]
		public void CustomFontImageIconShouldDisplay()
		{
			// Is a iOS issue; see https://github.com/dotnet/maui/issues/8244
			App.WaitForElement("TabBarIcon");
			VerifyScreenshot();
		}
	}
}
#endif