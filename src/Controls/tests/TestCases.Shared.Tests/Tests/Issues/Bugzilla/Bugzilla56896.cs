using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla56896 : _IssuesUITest
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
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		public void ListViewsWithManyElementsPerformanceCheck()
		{
			App.WaitForElement(InstructionsId);
			App.WaitForElement(ConstructorCountId);
			App.WaitForElement(TimeId);
			int.TryParse(App.WaitForElement(ConstructorCountId).GetText(), out int count);
			ClassicAssert.IsTrue(count < 100); // Failing test makes ~15000 constructor calls
			int.TryParse(App.WaitForElement(TimeId).GetText(), out int time);
			ClassicAssert.IsTrue(count < 2000); // Failing test takes ~4000ms
		}
	}
}