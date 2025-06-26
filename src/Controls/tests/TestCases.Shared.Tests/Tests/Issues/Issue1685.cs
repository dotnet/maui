
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1685 : _IssuesUITest
	{
		const string ButtonId = "Button1685";
		const string Success = "Success";

		public Issue1685(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Entry clears when updating text from native with one-way binding";

		[Fact]
		[Trait("Category", UITestCategories.Entry)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void EntryOneWayBindingShouldUpdate()
		{
			App.WaitForElement(ButtonId);
			App.Tap(ButtonId);
			Assert.That(App.FindElement("TestEntry").GetText(), Is.EqualTo(Success));
		}
	}
}
