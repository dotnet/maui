namespace Maui.Controls.Sample.Issues;

public partial class Issue24741TabbedPage : TabbedPage
{
	public Issue24741TabbedPage()
	{
		InitializeComponent();
	}
}

public class Issue24741Page1 : ContentPage
{
	public Issue24741Page1()
	{
		Title = "Page 1";

		var content = new Button
		{
			AutomationId = "Page1Button",
			Text = "Page 1"
		};

		content.Clicked += async (sender, args) =>
		{
			await Navigation.PopAsync();
		};

		Content = new VerticalStackLayout
		{
			content
		};
	}
}

public class Issue24741Page2 : ContentPage
{
	public Issue24741Page2()
	{
		Title = "Page 2";

		var content = new Button
		{
			AutomationId = "Page2Button",
			Text = "Page 2"
		};

		content.Clicked += async (sender, args) =>
		{
			await Navigation.PopAsync();
		};

		Content = new VerticalStackLayout
		{
			content
		};
	}
}