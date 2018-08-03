using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Collections.ObjectModel;
using System.Threading;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 3367, "The ScrollTo method freezes UI", PlatformAffected.UWP)]
	public class Issue3367 : TestContentPage 
	{
		bool simulateMessages = false;
		bool simulatedThreadEnabled = true;

		protected override void Init ()
		{
			var collection = new ObservableCollection<string>();
			int itemId = 0;
			for (int i = 0; i < 100; i++)
				collection.Add($"message {++itemId}");
			BindingContext = collection;
			var lstMessages = new ListView();
			lstMessages.SetBinding(ListView.ItemsSourceProperty, ".");

			new Thread(() =>
			{
				while (simulatedThreadEnabled)
				{
					Thread.Sleep(500);
					if (!simulateMessages)
						continue;

					var newItem = $"added message {++itemId}";
					collection.Add(newItem);
					Device.BeginInvokeOnMainThread(() => lstMessages.ScrollTo(newItem, ScrollToPosition.Start, false));
				}
			}).Start();
			
			this.Disappearing += (_, __) => simulatedThreadEnabled = false;

			var startStopButton = new Button()
			{
				Text = "Start/stop simulate",
				Command = new Command(() => simulateMessages = !simulateMessages)
			};

			Content = new StackLayout()
			{
				Children =
				{
					new Label { Text = "As the list views scrolls, click around and try to select rows in the ListView. " +
						"The UI should remain responsive as you click and as the ListView continues to scrolls." },
					startStopButton,
					lstMessages
				}
			};
		}
	}
}
