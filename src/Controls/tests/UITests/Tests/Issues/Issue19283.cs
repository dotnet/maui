using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue19283 : _IssuesUITest
	{
		public Issue19283(TestDevice device) : base(device) { }

		public override string Issue => "PointerOver VSM Breaks Button";

		[Test]
		public void ButtonStillWorksWhenItHasPointerOverVSMSet()
		{
			App.Click("btn");
			App.WaitForElement("Success");
		}
	}
}