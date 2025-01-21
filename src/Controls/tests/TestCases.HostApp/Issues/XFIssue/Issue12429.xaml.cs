namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 12429, "[Bug] Shell flyout items have a minimum height", PlatformAffected.iOS)]
public partial class Issue12429 : TestShell
{
	public double SmallFlyoutItem { get; }
	public double SizeToModifyBy { get; }

	public Issue12429()
	{
		SmallFlyoutItem = 35d;
		SizeToModifyBy = 20d;

		InitializeComponent();

		// TODO: make this work
		if (DeviceInfo.Platform == DevicePlatform.Android)
			SmallFlyoutItem = SmallFlyoutItem / DeviceDisplay.MainDisplayInfo.Density;

		if (DeviceInfo.Platform == DevicePlatform.Android)
			SizeToModifyBy = SizeToModifyBy / DeviceDisplay.MainDisplayInfo.Density;

		this.BindingContext = this;
	}

	protected override void Init()
	{
	}

	void ResizeFlyoutItem(System.Object sender, System.EventArgs e)
	{
		((sender as Element).Parent as VisualElement).HeightRequest += SizeToModifyBy;
	}

	void ResizeFlyoutItemDown(System.Object sender, System.EventArgs e)
	{
		((sender as Element).Parent as VisualElement).HeightRequest -= SizeToModifyBy;
	}
}

public class Issue12429Page : ContentPage
{
	public Issue12429Page()
	{
		Background = SolidColorBrush.White;
		var label = new Label
		{
			Text = "Flyout Item 1: Explicit Height of 35, Flyout Item 2: will grow and shrink when you click the buttons, Flyout Item 3: doesn't exist, and Flyout Item 4: uses the default platform sizes.",
			VerticalTextAlignment = TextAlignment.Center,
			TextColor = Colors.Black,
			AutomationId = "PageLoaded"
		};

		Content = new StackLayout()
		{
			Children =
			{
				new Label
				{
					Text = "Flyout Item 1: Explicit Height of 35.",
					VerticalTextAlignment = TextAlignment.Center,
					TextColor = Colors.Black,
					AutomationId = "PageLoaded"
				},
				new Label
				{
					Text = "Flyout Item 2: Height sizes to the content.",
					VerticalTextAlignment = TextAlignment.Center,
					TextColor = Colors.Black
				},
				new Label
				{
					Text = "Flyout Item 3: will grow and shrink when you click the buttons.",
					VerticalTextAlignment = TextAlignment.Center,
					TextColor = Colors.Black
				},
				new Label
				{
					Text = "Flyout Item 4: doesn't exist. You should only see 4 Flyout Items",
					VerticalTextAlignment = TextAlignment.Center,
					TextColor = Colors.Black
				},
				new Label
				{
					Text = "Flyout Item 5: uses the default height if no templates are used.",
					VerticalTextAlignment = TextAlignment.Center,
					TextColor = Colors.Black
				}
			}
		};
	}
}