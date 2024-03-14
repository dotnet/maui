using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla39668 : IssuesUITest
	{
		public Bugzilla39668(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Overriding ListView.CreateDefault Does Not Work on Windows";

		[Test]
		[Category(UITestCategories.ListView)]
		[FailsOnIOS]
		public void Bugzilla39668Test()
		{
			RunningApp.WaitForElement("Success");
		}
	}
}