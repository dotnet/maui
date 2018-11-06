using System;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 42069, "Garbage Collector can not collect pages that use ImageSource as a StaticResource",
		PlatformAffected.All)]
	public class Bugzilla42069 : TestNavigationPage
	{
		protected override void Init()
		{
			if (Application.Current.Resources == null)
			{
				Application.Current.Resources = new ResourceDictionary();
			}

			if (!Application.Current.Resources.ContainsKey("SomeSmallImage"))
			{
				ImageSource smallImage;
				switch (Device.RuntimePlatform) {
				default:
					smallImage = "coffee.png";
					break;
				case Device.UWP:
					smallImage = "bank.png";
					break;
				}

				Application.Current.Resources.Add("SomeSmallImage", smallImage);
			}

			const string instructions1 = @"Tap the Start button and follow the instructions on the next page.";
			string instructions2 =
				$"When you return to this page, tap the Collect button. The message \n'{Bugzilla42069_Page.DestructorMessage}'\n should appear at least once in the debug output.";

			var label1 = new Label { Text = instructions1 };
			var label2 = new Label { Text = instructions2, HorizontalTextAlignment = TextAlignment.Center };

			var startButton = new Button { Text = "Start" };
			startButton.Clicked += (sender, args) =>
			{
				// We have to do the push-pop-push dance because NavigationPage
				// holds a reference to its last page for unrelated reasons; our concern 
				// here is that the first Bugzilla42069_Page that we pushed gets collected
				PushAsync(new Bugzilla42069_Page(), false);
				PopAsync(false);
				PushAsync(new Bugzilla42069_Page(), false);
			};

			var collectButton = new Button { Text = "Collect" };
			collectButton.Clicked += (sender, args) =>
			{
				GC.Collect();
				GC.Collect();
				GC.Collect();
			};

			var startPage = new ContentPage
			{
				Content = new StackLayout
				{
					Children =
					{
						label1,
						startButton,
						label2,
						collectButton
					}
				}
			};

			PushAsync(startPage);
		}
	}
}
