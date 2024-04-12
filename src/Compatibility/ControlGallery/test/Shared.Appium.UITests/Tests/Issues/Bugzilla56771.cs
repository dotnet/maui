#if IOS
using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla56771 : IssuesUITest
	{
		const string Success = "Success";
		const string BtnAdd = "btnAdd";

		public Bugzilla56771(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Multi-item add in INotifyCollectionChanged causes a NSInternalInconsistencyException in bindings on iOS";

		[Test]
		[Category(UITestCategories.ListView)]
		[FailsOnIOS]
		public void AppDoesntCrashWhenResettingPage()
		{
			RunningApp.WaitForElement(BtnAdd);
			RunningApp.Tap(BtnAdd);
			RunningApp.WaitForNoElement(Success);
		}
	}
}
#endif