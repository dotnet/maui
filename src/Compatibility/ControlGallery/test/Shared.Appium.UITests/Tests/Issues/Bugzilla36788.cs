#if IOS
using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla36788 : IssuesUITest
	{
		public Bugzilla36788(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Truncation Issues with Relative Layouts";

		[Test]
		[Category(UITestCategories.Layout)]
		[FailsOnAndroid]
		[FailsOnIOS]
		public void Bugzilla36788Test()
		{
			RunningApp.WaitForNoElement("Passed");
		}
	}
}
#endif