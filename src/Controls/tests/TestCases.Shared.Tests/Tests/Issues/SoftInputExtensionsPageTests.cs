using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class SoftInputExtensionsPageTests : _IssuesUITest
	{
		public SoftInputExtensionsPageTests(TestDevice device) : base(device) { }

		public override string Issue => "Soft Input Extension Methods";

		[Fact]
		[Trait("Category", UITestCategories.Entry)]
		public void SoftInputExtensionsPageTest()
		{
			// Make sure the buttons appear on the screen.
			Task.Delay(1000).Wait();
			App.Tap("ShowKeyboard");
			Assert.True(App.WaitForTextToBePresentInElement("Result", "True"));
			App.Tap("HideKeyboard");
			Assert.True(App.WaitForTextToBePresentInElement("Result", "False"));
		}
	}
}
