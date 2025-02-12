using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27627 : _IssuesUITest
	{
		public override string Issue => "[Windows] Border Automation Peer";

		public Issue27627(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.Border)]
		public void VerifyBorderAutomationPeer()
		{
			App.WaitForElement("TestBorder");
		}
	}
}
