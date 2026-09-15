namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35844, "Shell TitleView does not resize after rotation on iOS 26+", PlatformAffected.iOS)]
public class Issue35844Shell : Shell
{
	public Issue35844Shell()
	{
		Issue35844 contentPage = new Issue35844();

		ShellContent shellContent = new ShellContent
		{
			Content = contentPage,
			Route = "Issue35844"
		};

		Items.Add(shellContent);
	}
}

public class Issue35844 : ContentPage
{
	public Issue35844()
	{
		Shell.SetTitleView(this, new Grid
		{
			BackgroundColor = Colors.LightBlue,
			AutomationId = "TitleViewGrid",
			HorizontalOptions = LayoutOptions.Fill,
			Children =
			{
				new Label
				{
					Text = "Shell TitleView",
					AutomationId = "TitleLabel",
					TextColor = Colors.White,
					FontSize = 18,
					FontAttributes = FontAttributes.Bold,
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center
				}
			}
		});

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Spacing = 10,
			Children =
			{
				new Label
				{
					Text = "Issue 35844",
					FontSize = 20,
					FontAttributes = FontAttributes.Bold,
					AutomationId = "HeaderLabel",
					HorizontalOptions = LayoutOptions.Center
				},
				new Label
				{
					Text = "Shell TitleView should fill the navigation bar width after rotation on iOS 26+.",
					FontSize = 14,
					AutomationId = "DescriptionLabel"
				},
				new Label
				{
					Text = "Rotate device to test",
					AutomationId = "StatusLabel",
					FontSize = 16,
					TextColor = Colors.Gray
				}
			}
		};
	}
}
