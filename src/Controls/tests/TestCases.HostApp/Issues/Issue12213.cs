namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 12213, "[Windows] TapGestureRecognizer not working on Entry", PlatformAffected.UWP)]
	public class Issue12213 : TestContentPage
	{
		public Issue12213()
		{
		}

		protected override void Init()
		{
			var stackLayout = new StackLayout() { AutomationId = "StackLayout" };

			// Register a tap gesture recognizer for the stack layout.
			{
				var tapGestureRecognizer = new TapGestureRecognizer();
				tapGestureRecognizer.Tapped += StackLayoutTapGestureRecognizer_Tapped;
				tapGestureRecognizer.NumberOfTapsRequired = 1;
				stackLayout.GestureRecognizers.Add(tapGestureRecognizer);
			}

			// Add a button to the stack layout.
			{
				var button = new Button();
				button.Text = "No operation button";
				button.AutomationId = "Button";
				stackLayout.Children.Add(button);
			}

			// Add an entry to the stack layout.
			{
				var tapGestureRecognizer = new TapGestureRecognizer();
				tapGestureRecognizer.Tapped += EntryTapGestureRecognizer_Tapped;
				tapGestureRecognizer.NumberOfTapsRequired = 1;

				var entry = new Entry();
				entry.Placeholder = "Enter Your Name";
				entry.AutomationId = "Entry";
				entry.GestureRecognizers.Add(tapGestureRecognizer);
				stackLayout.Children.Add(entry);
			}

			Content = stackLayout;
		}

		private void StackLayoutTapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
		{
			Label label = new Label { AutomationId = "StackLayoutTapped", Text = "Non-entry tapped" };
			if (Content is Layout layout)
			{
				layout.Children.Add(label);
			}
		}

		private void EntryTapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
		{
			Label label = new Label { AutomationId = "EntryTapped", Text = "Entry tapped" };
			if (Content is Layout layout)
			{
				layout.Children.Add(label);
			}
		}
	}
}
