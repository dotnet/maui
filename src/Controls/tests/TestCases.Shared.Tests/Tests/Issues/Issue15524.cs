#if ANDROID
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
using static System.Net.Mime.MediaTypeNames;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue15524 : _IssuesUITest
	{
		public override string Issue => "Text entry border disappear when changing to/from dark mode in Android";

		public Issue15524(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyGreyShades()
		{
			App.WaitForElement("entry");
			App.Tap("changeTheme");
			VerifyScreenshot();
		}

	}
}
#endif
