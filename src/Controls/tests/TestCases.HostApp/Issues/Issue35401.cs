using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35401, "Modal page keyboard dismissal causes parent Shell page layout to offset on iOS", PlatformAffected.iOS)]
public class Issue35401 : Shell
{
	public Issue35401()
	{
		Routing.RegisterRoute(nameof(Issue35401Level2Page), typeof(Issue35401Level2Page));
		Routing.RegisterRoute(nameof(Issue35401ModalPage), typeof(Issue35401ModalPage));

		var tab = new Tab { Title = "Main" };
		tab.Items.Add(new ShellContent
		{
			Title = "Main",
			ContentTemplate = new DataTemplate(() => new Issue35401Level1Page())
		});
		Items.Add(tab);
	}
}

public class Issue35401Level1Page : ContentPage
{
	public Issue35401Level1Page()
	{
		Title = "Level1";
		Shell.SetBackButtonBehavior(this, new BackButtonBehavior { IsVisible = false });

		var scrollContent = new VerticalStackLayout { Padding = 16, Spacing = 10 };
		scrollContent.Add(new Button
		{
			Text = "Go to Level2",
			AutomationId = "GoToLevel2Button",
			Command = new Command(async () => await Shell.Current.GoToAsync(nameof(Issue35401Level2Page), false))
		});
		for (int i = 1; i <= 20; i++)
			scrollContent.Add(new Label { Text = $"Item {i:D2}", FontSize = 16 });

		Content = new ScrollView { Content = scrollContent };
	}
}

public class Issue35401Level2Page : ContentPage
{
	public Issue35401Level2Page()
	{
		Title = "Level2";

		var titleLabel = new Label
		{
			Text = "Level2 Page",
			FontSize = 24,
			FontAttributes = FontAttributes.Bold,
			AutomationId = "Level2Title"
		};

		var scrollContent = new VerticalStackLayout { Padding = 16, Spacing = 10 };
		scrollContent.Add(titleLabel);
		scrollContent.Add(new Button
		{
			Text = "Open Modal",
			AutomationId = "OpenModalButton",
			Command = new Command(async () => await Shell.Current.GoToAsync(nameof(Issue35401ModalPage), false))
		});
		for (int i = 1; i <= 20; i++)
			scrollContent.Add(new Label { Text = $"Detail Item {i:D2}", FontSize = 16 });

		Content = new ScrollView { Content = scrollContent };
	}
}

public class Issue35401ModalPage : ContentPage
{	public Issue35401ModalPage()
	{
		Shell.SetPresentationMode(this, PresentationMode.ModalNotAnimated);
		BackgroundColor = Color.FromArgb("#80000000");

		var entry = new Entry
		{
			Placeholder = "Enter text here",
			HeightRequest = 120,
			AutomationId = "ModalEntry"
		};

		var closeButton = new Button
		{
			Text = "Close",
			AutomationId = "CloseModalButton",
			Command = new Command(async () => await Shell.Current.GoToAsync("..", false))
		};

		Content = new Grid
		{
			Children =
			{
				new Border
				{
					Margin = new Thickness(16),
					BackgroundColor = Colors.White,
					StrokeShape = new RoundRectangle { CornerRadius = 16 },
					StrokeThickness = 0,
					HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Center,
					Content = new VerticalStackLayout
					{
						Padding = 16,
						Spacing = 12,
						Children = { entry, closeButton }
					}
				}
			}
		};
	}
}
