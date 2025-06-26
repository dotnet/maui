#if ANDROID // This test case only works on Android because Send Keys functionality is platform-specific and does not behave the same way on iOS, macOS, or Windows.
using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue5724 : _IssuesUITest
	{
		public Issue5724(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Next Moves To Next Entry and Done Closes Input View";

		[Fact]
		[Trait("Category", UITestCategories.Entry)]
		public async Task TappingNextMovesToNextElement()
		{
			// Send Keys only works on Android which is why we are ignoring these other platforms.

			App.WaitForElement("Entry1");
			App.Tap("Entry1");

			await Task.Yield();

			App.Tap("SendNext");
			await Task.Yield();

			Assert.True(App.IsFocused("Entry2"));
		}

#if TEST_FAILS_ON_ANDROID // More information: https://github.com/dotnet/maui/issues/28640
		[Fact]
		[Trait("Category", UITestCategories.SoftInput)]
		public async Task TappingDoneClosesKeyboard()
		{
			App.WaitForElement("EntryDone");
			App.Tap("EntryDone");

			await Task.Yield();
			Assert.True(App.IsKeyboardShown());
			App.Tap("SendDone");
			await Task.Yield();
			Assert.False(App.IsKeyboardShown());
		}
#endif
	}
}
#endif