namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 11655, "Label's HorizontalTextAlignment property is not updated properly at runtime", PlatformAffected.Android)]
	public class Issue11655 : TestContentPage
	{
		Label rtlLabel;
		Label label;

		protected override void Init()
		{
			VerticalStackLayout verticalStackLayout = new VerticalStackLayout() { Spacing = 25, VerticalOptions = LayoutOptions.Center };
			rtlLabel = new Label
			{
				FlowDirection = FlowDirection.RightToLeft,
				BackgroundColor = Colors.Gray,
				Text = "Label with RTL",
				WidthRequest = 200,
				HeightRequest = 50,
				HorizontalTextAlignment = TextAlignment.End,
				VerticalTextAlignment = TextAlignment.Center,
				HorizontalOptions = LayoutOptions.Center,
			};

			label = new Label
			{
				FlowDirection = FlowDirection.LeftToRight,
				BackgroundColor = Colors.Gray,
				Text = "Label with LTR",
				WidthRequest = 200,
				HeightRequest = 50,
				HorizontalTextAlignment = TextAlignment.End,
				VerticalTextAlignment = TextAlignment.Center,
				HorizontalOptions = LayoutOptions.Center,
			};
			Button button = new Button
			{
				AutomationId = "Button",
				Text = "Change alignment",
				HorizontalOptions = LayoutOptions.Center,
			};
			button.Clicked += ChangeAlignmentClicked;
			verticalStackLayout.Children.Add(rtlLabel);
			verticalStackLayout.Children.Add(label);
			verticalStackLayout.Children.Add(button);
			Content = verticalStackLayout;
		}

		private void ChangeAlignmentClicked(object sender, EventArgs e)
		{
			label.HorizontalTextAlignment = TextAlignment.Start;
			rtlLabel.HorizontalTextAlignment = TextAlignment.Start;
		}

	}
}