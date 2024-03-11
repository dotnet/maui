using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla56896 : IssuesUITest
	{
		const string InstructionsId = "InstructionsId";
		const string ConstructorCountId = "constructorCount";
		const string TimeId = "time";

		public Bugzilla56896(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ListViews for lists with many elements regressed in performance on iOS";

		[Test]
		[Category(UITestCategories.ListView)]
		public void ListViewsWithManyElementsPerformanceCheck()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement(InstructionsId);
			RunningApp.WaitForElement(ConstructorCountId);
			RunningApp.WaitForElement(TimeId);
			int.TryParse(RunningApp.WaitForElement(ConstructorCountId).GetText(), out int count);
			ClassicAssert.IsTrue(count < 100); // Failing test makes ~15000 constructor calls
			int.TryParse(RunningApp.WaitForElement(TimeId).GetText(), out int time);
			ClassicAssert.IsTrue(count < 2000); // Failing test takes ~4000ms
		}
	}
}