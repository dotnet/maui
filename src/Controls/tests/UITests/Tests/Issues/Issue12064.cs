using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue12064 : _IssuesUITest
	{
		public Issue12064(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Setter showing a Build Error when using XAML OnPlatform Markup Extension";

		[Test]
		public void Issue12064Test()
		{
			App.WaitForElement("TestLabel");
			VerifyScreenshot();
		}
	}
}
