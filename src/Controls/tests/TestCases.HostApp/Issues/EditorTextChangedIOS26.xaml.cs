namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 0, "Editor TextChanged event not firing on iOS 26.1 release build",
		PlatformAffected.iOS)]
	public partial class EditorTextChangedIOS26 : ContentPage
	{
		private int _eventCount = 0;

		public EditorTextChangedIOS26()
		{
			InitializeComponent();
		}

		private void OnEditorTextChanged(object sender, TextChangedEventArgs e)
		{
			_eventCount++;
			EventCountLabel.Text = $"TextChanged event count: {_eventCount}";
			LastTextLabel.Text = $"Last text: {(string.IsNullOrEmpty(e.NewTextValue) ? "(empty)" : e.NewTextValue)}";
		}
	}
}
