#if IOS  // The issue is specific to iOS, so restricting other platforms
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30147 : _IssuesUITest
{
	public override string Issue => "MauiScrollView resets ContentOffset on first layout pass";

	public Issue30147(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyScrollViewContentOffsetValue()
	{
		App.WaitForElement("OffsetLabel");
		Assert.That(App.FindElement("OffsetLabel").GetText(), Is.EqualTo("500"));
	}
}
#endif