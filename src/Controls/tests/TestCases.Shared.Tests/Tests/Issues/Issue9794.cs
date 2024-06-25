/*
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue9794 : _IssuesUITest
	{
		public Issue9794(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Tabbar Disappears with linker";
		
		[Test]
		[Category(UITestCategories.Shell)]
		[Category(UITestCategories.Compatibility)]
		public void EnsureTabBarStaysVisibleAfterPoppingPage()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			App.Tap("GoForward");
			App.Back();
			App.Tap("tab2");
			App.Tap("tab1");
			App.Tap("tab2");
			App.Tap("tab1");
			App.Tap("tab2");
			App.Tap("tab1");
		}
	}
}
*/