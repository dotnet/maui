#if TEST_FAILS_ON_WINDOWS //The issue with the label count not incrementing correctly may be caused by multiple instances of objects being created, leading to inconsistent label values. 
//For more information : https://github.com/dotnet/maui/issues/12090
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla45722 : _IssuesUITest
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
		[Category(UITestCategories.Compatibility)]
		public void LabelsInListViewTemplatesShouldBeCollected()
		{
			App.WaitForElement(Update);

			for (int n = 0; n < 10; n++)
			{
				App.Tap(Update);
			}

			App.Tap(Collect);
			App.WaitForElement(Success);
		}
	}
}
#endif