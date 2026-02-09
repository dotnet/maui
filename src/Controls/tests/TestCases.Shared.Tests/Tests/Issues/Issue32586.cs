#if IOS || ANDROID
using System.Threading.Tasks;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32586 : _IssuesUITest
{
	public override string Issue => "[iOS] Layout issue using TranslateToAsync causes infinite property changed cycle";

	public Issue32586(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public async Task VerifyLayoutWithTranslateToAsync()
	{
		var label = App.WaitForElement("TestLabel");
		App.Tap("FooterButton");
		await Task.Delay(500); // Wait for the animation to complete
		App.Tap("FooterButton");
		await Task.Delay(500); // Wait for the animation to complete

		var labelText = label.GetText();
		Assert.That(labelText, Is.EqualTo("Footer is now hidden"));
	}
}
#endif