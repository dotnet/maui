using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;

#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
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

			var button = new Button { Text = ButtonText, BackgroundColor = Colors.CornflowerBlue };

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