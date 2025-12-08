using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue99999 : _IssuesUITest
{
	public Issue99999(TestDevice device) : base(device) { }

	public override string Issue => "[Windows, Android] ScrollView Content Not Removed When Set to Null";
	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewContentWhenSetToNull()
	{
		App.WaitForElement("SetNullButton");
		App.Tap("SetNullButton");
		App.WaitForNoElement("ContentLabel");
		App.Tap("AddContentButton");
		App.WaitForElement("ContentLabel");
	}
}