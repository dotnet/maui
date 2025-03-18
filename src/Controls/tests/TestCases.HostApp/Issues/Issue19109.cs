namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19109, "CursorPosition Property Not Applied Correctly to Entry Control on iOS Platform", PlatformAffected.iOS)]
	public partial class Issue19109: ContentPage
	{
		Entry entry;
		public Issue19109()
		{
			entry = new Entry
            {
                Text = "Entry 123",
                CursorPosition = 5,
                Placeholder = "Focus this entry to check if the cursor position is set correctly.",
                ReturnType = ReturnType.Next,
				AutomationId = "EntryControl"
            };

            var button = new Button
            {
                Text = "Focus Entry",
				AutomationId = "FocusButton"
            };

            var label = new Label
            {
                Text = "Cursor Position: 5",
                FontSize = 16,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center
            };

            button.Clicked += (sender, e) =>
            {
                entry.Focus();
            };

            Content = new StackLayout
            {
                Children = { entry, button, label },
                Padding = new Thickness(20),
                Spacing = 10
            };
			
		}
	}
}
