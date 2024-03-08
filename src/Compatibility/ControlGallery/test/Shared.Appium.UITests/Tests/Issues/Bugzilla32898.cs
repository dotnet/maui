using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla32898 : IssuesUITest
	{
		const string Success = "Success";
		const int Timeout = 20000;

		public Bugzilla32898(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Memory leak when TabbedPage is popped out ";

		[Test]
		[Category(UITestCategories.LifeCycle)]
		public void Issu32898Test()
		{
			var timeout = Timeout; // Give this a little slop to set the result text
			RunningApp.WaitForNoElement(Success, timeout: TimeSpan.FromMilliseconds(timeout));
		}
	}
}