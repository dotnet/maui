using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla34912 : _IssuesUITest
{
	public Bugzilla34912(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Can't change IsPresented when setting SplitOnLandscape ";

	// [Test]
	// [Category(UITestCategories.ListView)]
	// [FailsOnIOSWhenRunningOnXamarinUITest]
	// public void Bugzilla34912Test()
	// {
	// 	App.Tap("Allen");
	// 	App.WaitForElement("You tapped Allen");
	// 	App.Tap("OK");
	// 	App.Tap("btnDisable");
	// 	App.Tap("Allen");
	// 	App.WaitForNoElement("You tapped Allen");
	// }
}