#if TEST_FAILS_ON_ANDROID // Exception handling not performs in Android, Issue: https://github.com/dotnet/maui/issues/26941
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla56771 : _IssuesUITest
	{
		const string Success = "Success";
		const string BtnAdd = "btnAdd";

		public Bugzilla56771(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Multi-item add in INotifyCollectionChanged causes a NSInternalInconsistencyException in bindings on iOS";

		[Test]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Compatibility)]
		public void Bugzilla56771Test()
		{
			App.WaitForElement(BtnAdd);
			App.Tap(BtnAdd);
			App.WaitForElement(Success);
		}
	}
}
#endif