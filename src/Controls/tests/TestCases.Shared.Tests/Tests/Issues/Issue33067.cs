using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue33067 : _IssuesUITest
{
	public Issue33067(TestDevice device) : base(device) { }

	public override string Issue => "[Windows, Android] ScrollView Content Not Removed When Set to Null";

	[Test, Order(1)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewContentShouldNull()
    {
        App.WaitForElement("SetNullButton");
		App.WaitForNoElement("ContentLabel");
    }

	[Test, Order(2)]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewContentWhenSetToNull()
	{
		App.WaitForElement("SetNullButton");
		App.Tap("AddContentButton");
		App.WaitForElement("ContentLabel");
		App.Tap("SetNullButton");
		App.WaitForNoElement("ContentLabel");
	}
}