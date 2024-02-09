using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue20023 : _IssuesUITest
	{
		public Issue20023(TestDevice device) : base(device) { }
		public override string Issue => "RadioButton is not currently being Garbage Collected";

		[Test]
		public void RadioButtonRendersDefaultTemplate()
		{
			App.WaitForElement("WaitForStubControl");
			App.Screenshot("RadioButton is rendered using the default ControlTemplate");
		}
	}
}
