using Microsoft.Maui.Appium;
using Microsoft.Maui.AppiumTests;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests.Issues
{
	class Issue14557 : _IssuesUITest
	{
		public Issue14557(TestDevice device) : base(device) { }
		public override string Issue => "CollectionView header and footer not displaying on Windows";

		[Test]
		public void HeaderAndFooterRender()
		{
			// This issue is not working on net7 for the following platforms 
			// This is not a regression it's just the test being backported from net8
			UITestContext.IgnoreIfPlatforms(new[]
			{
				TestDevice.Android, TestDevice.Windows
			});

			App.WaitForElement("collectionView");

			var headerText = App.Query("headerLabel").First().Text;
			var footerText = App.Query("footerLabel").First().Text;

			Assert.IsNotEmpty(headerText);
			Assert.IsNotEmpty(footerText);
		}
	}
}
