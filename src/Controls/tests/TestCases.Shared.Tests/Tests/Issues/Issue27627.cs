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
			// Check whether the parent Border is found or not
			App.WaitForElement("TestBorder");

			// Check whether the nested Border is found or not
			App.WaitForElement("NestedBorder");

			// Check whether the Label inside the nested Border is found or not
			App.WaitForElement("TestLabel");
		}
	}
}
