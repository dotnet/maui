namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 2680, "[Enhancement] Add VerticalScrollMode/HorizontalScrollMode to ListView and ScrollView", PlatformAffected.All)]
	public class Issue2680ScrollView : TestContentPage // or TestFlyoutPage, etc ... 
	{
		public bool IsScrollEnabled { get; set; } = false;

		public void ToggleButtonText()
		{
			IsScrollEnabled = !IsScrollEnabled;
			toggleButton.Text = ButtonText;
		}

		public string ButtonText => IsScrollEnabled ? ButtonDisabledCaption : ButtonEnabledCaption;

		protected override void Init()
		{
			var contentView = new ContentView
			{
				HeightRequest = 2000
			};

			// Initialize ui here instead of ctor 
			var longStackLayout = new StackLayout();

			toggleButton = new Button { Text = ButtonText, AutomationId = ToggleButtonMark };
			toggleButton.Clicked += ToggleButtonOnClicked;

			longStackLayout.Children.Add(toggleButton);

			firstItemLabel = new Label
			{
				Text = "Not scrolled",
				AutomationId = FirstItemMark
			};

			longStackLayout.Children.Add(firstItemLabel);
			Enumerable.Range(2, 50).Select(i => new Label { Text = $"Test label {i}" })
				.ToList().ForEach(label => longStackLayout.Children.Add(label));

			contentView.Content = longStackLayout;

			scrollView = new ScrollView
			{
				Orientation = ScrollOrientation.Neither,
				AutomationId = ScrollViewMark,
				Content = contentView
			};

			scrollView.Scrolled += ScrollViewOnScrolled;
			Content = scrollView;
		}

		void ToggleButtonOnClicked(object sender, EventArgs e)
		{
			ToggleButtonText();
			scrollView.Orientation = IsScrollEnabled ? ScrollOrientation.Vertical : ScrollOrientation.Neither;
		}

		void ScrollViewOnScrolled(object sender, ScrolledEventArgs e)
		{
			firstItemLabel.Text = "Scrolled";
		}

		ScrollView scrollView;
		Button toggleButton;

		Label firstItemLabel;

		const string ScrollViewMark = "ScrollView";
		const string FirstItemMark = "FirstItem";
		const string ToggleButtonMark = "ToggleButton";

		const string ButtonDisabledCaption = "Disable scroll";
		const string ButtonEnabledCaption = "Enable scroll";
	}
}