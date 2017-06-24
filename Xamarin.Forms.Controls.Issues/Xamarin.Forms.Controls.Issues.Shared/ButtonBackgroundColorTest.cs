using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;

#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Button)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 57717, "Setting background color on Button in Android FormsApplicationActivity causes NRE", PlatformAffected.Android)]
	public class ButtonBackgroundColorTest : TestContentPage
	{
		const string ButtonText = "I am a button";

		protected override void Init()
		{
			var layout = new StackLayout();

			var instructions = new Label { Text = "If you can see this, the test has passed." };

			var button = new Button { Text = ButtonText, BackgroundColor = Color.CornflowerBlue };

			layout.Children.Add(instructions);
			layout.Children.Add(button);

			Content = layout;
		}

#if UITEST
		[Test]
		public void ButtonBackgroundColorAutomatedTest()
		{
			// With the original bug in place, we'll crash before we get this far
			RunningApp.WaitForElement(ButtonText);
		}
#endif
	}
}