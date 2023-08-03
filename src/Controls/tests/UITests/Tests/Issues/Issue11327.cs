using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue11327 : _IssuesUITest
	{
		public Issue11327(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Incorrect layout with GridItemsLayout and RefreshView";

		[Test]
		public void Issue11327Test()
		{
			App.WaitForElement("labeCell", "Timeout wait for labeCell", TimeSpan.FromSeconds(10));
			VerifyScreenshot();
		}
	}
}
