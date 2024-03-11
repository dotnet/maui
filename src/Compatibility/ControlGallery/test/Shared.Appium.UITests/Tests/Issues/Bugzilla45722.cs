using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla45722 : IssuesUITest
	{
		const string Success = "Success";
		const string Update = "Update List";
		const string Collect = "GC";

		public Bugzilla45722(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Memory leak in Xamarin Forms ListView";

		[Test]
		[Category(UITestCategories.Label)]
		[Category(UITestCategories.ListView)]
		public void LabelsInListViewTemplatesShouldBeCollected()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS, TestDevice.Mac]);

			RunningApp.WaitForElement(Update);

			for (int n = 0; n < 10; n++)
			{
				RunningApp.Tap(Update);
			}

			RunningApp.Tap(Collect);
			RunningApp.WaitForNoElement(Success);
		}
	}
}