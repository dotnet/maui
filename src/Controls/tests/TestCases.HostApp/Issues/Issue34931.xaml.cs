namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34931, "Shell flyout item template does not update selected visuals after DynamicResource changes", PlatformAffected.All)]
public partial class Issue34931 : TestShell
{
	internal const string InitialPrimaryColor = "#512BD4";
	internal const string UpdatedPrimaryColor = "#FF6347";

	public Issue34931()
	{
		if (Application.Current is not null)
			Application.Current.Resources["Primary"] = Color.FromArgb(InitialPrimaryColor);

		InitializeComponent();
		IncreaseFlyoutItemsHeightSoUITestsCanClickOnThem();
	}

	protected override void Init()
	{
	}
}

public class Issue34931MainPage : ContentPage
{
	readonly Label _currentColorLabel;
	readonly string[] _colors = [Issue34931.InitialPrimaryColor, Issue34931.UpdatedPrimaryColor];
	int _colorIndex;

	public Issue34931MainPage()
	{
		Title = "Home";

		_currentColorLabel = new Label
		{
			AutomationId = "CurrentColorLabel",
			FontSize = 14,
			HorizontalOptions = LayoutOptions.Center,
			Text = $"Current Primary: {Issue34931.InitialPrimaryColor}",
			TextColor = Colors.Black
		};

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(30, 0),
				Spacing = 25,
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						Text = "Flyout DynamicResource Issue",
						FontAttributes = FontAttributes.Bold,
						FontSize = 24,
						HorizontalOptions = LayoutOptions.Center,
					},
					new Label
					{
						Text = "1. Tap Change Theme Color to update the Primary DynamicResource at runtime.\n2. Open the flyout menu and switch between pages repeatedly.\n3. Reopen the flyout and verify the selected item reflects the updated color.",
						HorizontalOptions = LayoutOptions.Center,
						HorizontalTextAlignment = TextAlignment.Center,
					},
					new Button
					{
						AutomationId = "ChangeColorButton",
						HorizontalOptions = LayoutOptions.Fill,
						Text = "Change Theme Color"
					}.Assign(out var changeColorButton),
					_currentColorLabel,
				}
			}
		};

		changeColorButton.Clicked += OnChangeColorClicked;
	}

		void OnChangeColorClicked(object sender, EventArgs e)
		{
			_colorIndex = (_colorIndex + 1) % _colors.Length;
			var colorValue = _colors[_colorIndex];

			if (Application.Current is not null)
				Application.Current.Resources["Primary"] = Color.FromArgb(colorValue);

			_currentColorLabel.Text = $"Current Primary: {colorValue}";
		}
}

public class Issue34931SecondPage : ContentPage
{
	public Issue34931SecondPage()
	{
		Title = "Second";
		Content = new VerticalStackLayout
		{
			Padding = 30,
			Spacing = 20,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					Text = "Second Page",
					FontSize = 32,
					HorizontalOptions = LayoutOptions.Center,
				},
				new Label
				{
					AutomationId = "Issue34931SecondPageLabel",
					Text = "Open the flyout and switch between pages repeatedly to reproduce the issue.",
					HorizontalOptions = LayoutOptions.Center,
					HorizontalTextAlignment = TextAlignment.Center,
				}
			}
		};
	}
}

public class Issue34931ThirdPage : ContentPage
{
	public Issue34931ThirdPage()
	{
		Title = "Third";
		Content = new VerticalStackLayout
		{
			Padding = 30,
			Spacing = 20,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					Text = "Third Page",
					FontSize = 32,
					HorizontalOptions = LayoutOptions.Center,
				},
				new Label
				{
					AutomationId = "Issue34931ThirdPageLabel",
					Text = "The selected flyout item should keep the updated DynamicResource styling.",
					HorizontalOptions = LayoutOptions.Center,
					HorizontalTextAlignment = TextAlignment.Center,
				}
			}
		};
	}
}

static class Issue34931ViewExtensions
{
	public static T Assign<T>(this T view, out T assigned)
		where T : BindableObject
	{
		assigned = view;
		return view;
	}
}
