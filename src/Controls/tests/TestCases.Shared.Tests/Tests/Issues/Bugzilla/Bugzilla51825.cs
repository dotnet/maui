using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.SearchBar)]
public class Bugzilla51825 : _IssuesUITest
{
	public Bugzilla51825(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] Korean input in SearchBar doesn't work";

	[Test]
	public void Bugzilla51825Test()
	{
		App.WaitForElement("Bugzilla51825SearchBar");
		App.EnterText("Bugzilla51825SearchBar", "Hello");
		var label = App.WaitForFirstElement("Bugzilla51825Label");

		ClassicAssert.IsNotEmpty(label.ReadText());

		// Windows App Driver and the Search Bar are a bit buggy
		// It randomly doesn't enter the first letter
#if !WINDOWS
		ClassicAssert.AreEqual("Hello", label.ReadText());
#endif

		App.Tap("Bugzilla51825Button");

		var labelChange2 = App.WaitForFirstElement("Bugzilla51825Label");
		ClassicAssert.AreEqual("Test", labelChange2.ReadText());
	}
}