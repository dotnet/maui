using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3319 : _IssuesUITest
{
	public Issue3319(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] Clear and adding rows exception";

	//[Test]
	//[Category(UITestCategories.ListView)]
	//[FailsOnMauiIOS]
	//public void Issue3319Test()
	//{
	//	App.WaitForElement(q => q.Marked("Will this repo work?"));

	//}
}