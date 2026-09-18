using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue32221 : _IssuesUITest
{
	public Issue32221(TestDevice device) : base(device) { }

	public override string Issue => "[iOS] ScrollView does not resize when children are removed from StackLayout at runtime";
	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewHeightWhenRemoveChildAtRuntime()
	{
		App.WaitForElement("AddLabelButton");
		App.Tap("AddLabelButton");
		App.Tap("RemoveLabelButton");
		VerifyScreenshot();
	}
}