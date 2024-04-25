using NUnit.Framework;
using OpenQA.Selenium.Appium.Android;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue5724 : _IssuesUITest
	{
		public Issue5724(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Next Moves To Next Entry and Done Closes Input View";

		[Test]
		public async Task TappingNextMovesToNextElement()
		{
			this.IgnoreIfPlatforms(new[]
			{
				TestDevice.Mac,
				TestDevice.iOS,
				TestDevice.Windows,
			}, "Send Keys only works on Android which is why we are ignoring these other platforms");

			App.WaitForElement("Entry1");
			App.Tap("Entry1");

			await Task.Yield();

			App.Tap("SendNext");
			await Task.Yield();

			Assert.True(App.IsFocused("Entry2"));
		}

		[Test]
		public async Task TappingDoneClosesKeyboard()
		{
			this.IgnoreIfPlatforms(new[]
			{
				TestDevice.Mac,
				TestDevice.iOS,
				TestDevice.Windows,
			}, "Send Keys only works on Android which is why we are ignoring these other platforms");

			App.WaitForElement("EntryDone");
			App.Tap("EntryDone");

			await Task.Yield();
			Assert.True(App.IsKeyboardShown());
			App.Tap("SendDone");
			await Task.Yield();
			Assert.False(App.IsKeyboardShown());
		}
	}
}
