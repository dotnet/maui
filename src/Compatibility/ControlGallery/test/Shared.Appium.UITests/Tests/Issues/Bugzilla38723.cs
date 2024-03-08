using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla38723 : IssuesUITest
	{
		public Bugzilla38723(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Update Content in Picker's SelectedIndexChanged event causes NullReferenceException";

		[Test]
		[Category(UITestCategories.Picker)]
		public void Bugzilla38723Test()
		{
			RunningApp.Tap("SELECT");
			RunningApp.WaitForNoElement("Selected");
			RunningApp.WaitForNoElement("SELECT");
		}
	}
}