using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Gestures)]
	[Category(UITestCategories.IsEnabled)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 36703, 
		"TapGestureRecognizer inside initially disable Image will never fire Tapped event", PlatformAffected.All)]
	public class Bugzilla36703 : TestContentPage
	{
		const string TestImage = "testimage";
		const string Success = "Success";
		const string Toggle = "toggle";
		const string Testing = "Testing...";

		protected override void Init()
		{
			var image = new Image { Source = "coffee.png", IsEnabled = false, AutomationId = TestImage };
			var button = new Button { Text = $"Toggle IsEnabled (now {image.IsEnabled})", AutomationId = Toggle };
			var resultLabel = new Label { Text = "Testing..."};
			var instructions = new Label { Text = $"Tap the image. The '{Testing}' label should remain unchanged. " 
				+ $"Tap the 'Toggle IsEnabled' button. Now tap the image again." 
				+ $" The {Testing} Label should change its text to {Success}." };
			
			button.Clicked += (sender, args) =>
			{
				image.IsEnabled = !image.IsEnabled;
				button.Text = $"Toggle IsEnabled (now {image.IsEnabled})";
			};

			Content = new StackLayout
			{
				Padding = new Thickness(0, 20, 0, 0),
				Children =
				{
					instructions, resultLabel,
					image, button
				}
			};

			var tapGestureRecognizer = new TapGestureRecognizer();
			tapGestureRecognizer.Tapped += delegate
			{
				resultLabel.Text = Success;
			};

			image.GestureRecognizers.Add(tapGestureRecognizer);
		}

#if UITEST
		[Test]
		public void _36703Test()
		{
			RunningApp.WaitForElement(TestImage);
			RunningApp.Tap(TestImage);
			RunningApp.WaitForElement(Testing);
			RunningApp.Tap(Toggle);
			RunningApp.Tap(TestImage);
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}