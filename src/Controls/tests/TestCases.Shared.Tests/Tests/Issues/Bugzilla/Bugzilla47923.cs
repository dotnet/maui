#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // This test uses a vector image resources which applicable only for Android.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla47923 : _IssuesUITest
{
	public Bugzilla47923(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Vectors don\'t work in Images, and work badly in Buttons";


	[Test]
	[Category(UITestCategories.ListView)]
	public void Bugzilla47923Test()
	{
		foreach (var testString in new[] { "AspectFit", "AspectFill", "Fill", "Test cell views" })
		{
			App.WaitForElement((testString));
			App.Tap((testString));
			App.Back();
		}
	}
}
#endif