using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 24769, "Layout cycle when Progress Bar is in a ListView", PlatformAffected.WinPhone | PlatformAffected.WinRT)]
	public class Bugzilla24769 : TestContentPage
	{
		protected override void Init ()
		{
			var instructions = new Label () {
				Text = @"Click the button labeled 'Progress++' three times. Each time, all four ProgressBar controls should increment. If they do not increment, the test has failed."
			};

			var items = new List<ListItem> {
				new ListItem { Name = "Item1" },
				new ListItem { Name = "Item2" },
				new ListItem { Name = "Item3" }
			};

			var btn = new Button {
				Text = "Progress++"
			};

			var progressBar = new ProgressBar { Progress = 0.1 };

			btn.Clicked += (sender, arg) => {
				MessagingCenter.Send (this, "set_progress");
				progressBar.Progress += 0.1;
			};

			var list = new ListView {
				ItemsSource = items,
				ItemTemplate = new DataTemplate (typeof(ListCell))
			};

			BackgroundColor = Color.Maroon;

			Content = new StackLayout {
				VerticalOptions = LayoutOptions.Fill,
				HorizontalOptions = LayoutOptions.Fill,
				Children = {
					instructions,
					btn,
					progressBar,
					list
				}
			};
		}
	}

	internal class ListCell : ViewCell
	{
		public ListCell ()
		{
			var label = new Label ();
			label.SetBinding (Label.TextProperty, "Name");

			var progress = new ProgressBar { Progress = 0.1 };

			MessagingCenter.Subscribe<Bugzilla24769> (this, "set_progress", sender => { progress.Progress += 0.1; });

			View =
				new StackLayout {
					HorizontalOptions = LayoutOptions.Fill,
					BackgroundColor = Color.Gray,
					Children = {
						label,
						progress
					}
				};
		}
	}

	internal class ListItem
	{
		public string Name { get; set; }
	}
}
