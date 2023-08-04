using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue11363 : _IssuesUITest
	{
		public Issue11363(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "CollectionView inside RefreshView freezes app completely";

		[Test]
		public void Issue11327Test()
		{
			App.WaitForElement("labeCell", "Timeout wait for labeCell", TimeSpan.FromSeconds(10));
			App.ScrollDown("20");
			App.Tap("alert");
			App.WaitForElement("OK");
		}
	}
}
