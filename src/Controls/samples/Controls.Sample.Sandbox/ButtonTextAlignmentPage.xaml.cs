using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public partial class ButtonTextAlignmentPage : ContentPage
{
	readonly Button[] _scenarioButtons;
	readonly FontImageSource _previewIcon = new FontImageSource
	{
		Glyph = "+",
		Size = 24,
		Color = Colors.White
	};
	int _clickCount;

	public ButtonTextAlignmentPage()
	{
		InitializeComponent();

		_scenarioButtons = new[]
		{
			TextPreview, ImageTextPreview, SingleLineButton,
			MultilineButton, WrappedButton, TruncatedButton, ImageOnlyButton
		};

		ImageOnlyButton.ImageSource = new FontImageSource
		{
			Glyph = "+",
			Size = 24,
			Color = Colors.White
		};

		ImagePositionPicker.ItemsSource = new[]
		{
			Button.ButtonContentLayout.ImagePosition.Left,
			Button.ButtonContentLayout.ImagePosition.Top,
			Button.ButtonContentLayout.ImagePosition.Right,
			Button.ButtonContentLayout.ImagePosition.Bottom
		};
		ImagePositionPicker.SelectedIndex = 0;
		LineBreakPicker.ItemsSource = new[]
		{
			LineBreakMode.WordWrap,
			LineBreakMode.CharacterWrap,
			LineBreakMode.NoWrap,
			LineBreakMode.HeadTruncation,
			LineBreakMode.MiddleTruncation,
			LineBreakMode.TailTruncation
		};
		LineBreakPicker.SelectedIndex = 0;
		HorizontalAlignmentPicker.ItemsSource = new[] { "Native default", "Start", "Center", "End" };
		VerticalAlignmentPicker.ItemsSource = new[] { "Native default", "Start", "Center", "End" };
		HorizontalAlignmentPicker.SelectedIndex = VerticalAlignmentPicker.SelectedIndex = 0;
#if !ANDROID && !IOS && !WINDOWS
		HorizontalAlignmentPicker.IsEnabled = VerticalAlignmentPicker.IsEnabled = false;
#endif

		PreviewTextEditor.TextChanged += OnSettingsChanged;
		HorizontalAlignmentPicker.SelectedIndexChanged += OnSettingsChanged;
		VerticalAlignmentPicker.SelectedIndexChanged += OnSettingsChanged;
		ImagePositionPicker.SelectedIndexChanged += OnSettingsChanged;
		LineBreakPicker.SelectedIndexChanged += OnSettingsChanged;
		ShowImageSwitch.Toggled += OnSettingsChanged;
		EnabledSwitch.Toggled += OnSettingsChanged;
		RtlSwitch.Toggled += OnSettingsChanged;
		PaddingSwitch.Toggled += OnSettingsChanged;
		LargeTextSwitch.Toggled += OnSettingsChanged;

		UpdateScenarios();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		Console.WriteLine("SANDBOX: Button text alignment page appeared.");
	}

	void OnSettingsChanged(object? sender, EventArgs e) => UpdateScenarios();

	void UpdateScenarios()
	{
		if (ImagePositionPicker.SelectedItem is not Button.ButtonContentLayout.ImagePosition position ||
			LineBreakPicker.SelectedItem is not LineBreakMode lineBreakMode)
		{
			throw new InvalidOperationException("Select an image position and line break mode for the Button scenarios.");
		}

		var flowDirection = RtlSwitch.IsToggled ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
		var padding = PaddingSwitch.IsToggled ? new Thickness(32, 8, 8, 24) : new Thickness(16);
		var fontSize = LargeTextSwitch.IsToggled ? 26 : 18;

		ScenarioLayout.FlowDirection = flowDirection;
		ComparisonLayout.FlowDirection = flowDirection;
		foreach (var button in _scenarioButtons)
		{
			button.IsEnabled = EnabledSwitch.IsToggled;
			button.Padding = padding;
			button.FontSize = fontSize;
			UpdateAlignment(button, Button.HorizontalTextAlignmentProperty, HorizontalAlignmentPicker.SelectedIndex);
			UpdateAlignment(button, Button.VerticalTextAlignmentProperty, VerticalAlignmentPicker.SelectedIndex);
		}

		TextPreview.Text = ImageTextPreview.Text = PreviewTextEditor.Text;
		TextPreview.LineBreakMode = ImageTextPreview.LineBreakMode = lineBreakMode;
		ImageTextPreview.ContentLayout = new Button.ButtonContentLayout(position, 8);
		ImageTextPreview.ImageSource = ShowImageSwitch.IsToggled ? _previewIcon : null;

		SettingsLabel.Text = $"Horizontal: {HorizontalAlignmentPicker.SelectedItem}; vertical: {VerticalAlignmentPicker.SelectedItem}; " +
			$"direction: {flowDirection}; image: {position}; visible: {ShowImageSwitch.IsToggled}; " +
			$"wrapping: {lineBreakMode}; padding: {padding}; font: {fontSize}; enabled: {EnabledSwitch.IsToggled}";
		Console.WriteLine($"SANDBOX: Button alignment settings: {SettingsLabel.Text}");
	}

	static void UpdateAlignment(Button button, BindableProperty property, int selectedIndex)
	{
		if (selectedIndex == 0)
			button.ClearValue(property);
		else
			button.SetValue(property, selectedIndex switch
			{
				1 => TextAlignment.Start,
				2 => TextAlignment.Center,
				3 => TextAlignment.End,
				_ => throw new InvalidOperationException("Select a Button text alignment.")
			});
	}

	void OnScenarioClicked(object? sender, EventArgs e)
	{
		if (sender is Button button)
		{
			ClickStatusLabel.Text = $"Clicks: {++_clickCount}; last: {button.AutomationId}";
			Console.WriteLine($"SANDBOX: Button alignment clicked: {button.AutomationId}; count={_clickCount}");
		}
	}
}
