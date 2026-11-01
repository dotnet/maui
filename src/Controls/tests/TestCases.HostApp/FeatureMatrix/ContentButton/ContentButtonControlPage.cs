namespace Maui.Controls.Sample;

public class ContentButtonControlPage : ContentPage
{
	readonly ContentButton _button;
	readonly VerticalStackLayout _buttons = new() { Spacing = 12 };
	readonly Label _status = new() { AutomationId = "ContentButtonStatus" };
	int _commands, _clicks, _pressed, _released;
	string _order = "";
	string _pressAction = "";

	public ContentButtonControlPage()
	{
		_button = new ContentButton
		{
			AutomationId = "ContentButtonControl",
			WidthRequest = 260,
			HeightRequest = 120,
			Command = new Command(() =>
			{
				_commands++;
				_order = "Command";
				UpdateStatus();
			}),
		};
		SemanticProperties.SetDescription(_button, "Content button example");
		_button.Clicked += (_, _) =>
		{
			_clicks++;
			_order += ",Clicked";
			UpdateStatus();
		};
		_button.Pressed += (_, _) =>
		{
			_pressed++;
			if (_pressAction == "Disable")
				_button.IsEnabled = false;
			else if (_pressAction == "Remove")
				_buttons.Remove(_button);
			UpdateStatus();
		};
		_button.Released += (_, _) => { _released++; UpdateStatus(); };

		_buttons.Add(_button);
		_buttons.Add(new BoxView { HeightRequest = 500, Color = Colors.LightBlue });
		var actions = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap };
		AddAction(actions, "Reset", Reset);
		AddAction(actions, "DisableOnPress", () => _pressAction = "Disable");
		AddAction(actions, "RemoveOnPress", () => _pressAction = "Remove");
		AddAction(actions, "FocusButton", () => _button.Focus());
		AddAction(actions, "ReplaceContent", () => _button.Content = new Label { Text = "Replacement", BackgroundColor = Colors.Gold });
		AddAction(actions, "ClearContent", () => _button.Content = null);
		AddAction(actions, "Reconnect", () =>
		{
			_buttons.Remove(_button);
			_button.Handler?.DisconnectHandler();
			_button.Handler = null;
			_buttons.Insert(0, _button);
		});
		AddAction(actions, "ChangeStyle", () =>
		{
			_button.BorderWidth = 4;
			_button.CornerRadius = 24;
			_button.Background = new LinearGradientBrush(new GradientStopCollection
			{
				new GradientStop(Colors.Red, 0),
				new GradientStop(Colors.Blue, 1),
			}, new Point(0, 0), new Point(1, 1));
		});

		var layout = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star),
			}
		};
		layout.Add(_status);
		layout.Add(actions, 0, 1);
		layout.Add(new ScrollView { AutomationId = "ContentButtonScroll", Content = _buttons, HeightRequest = 240 }, 0, 2);
		Content = layout;
		Reset();
	}

	static void AddAction(FlexLayout layout, string name, Action action)
	{
		var button = new Button { Text = name, AutomationId = name, FontSize = 12 };
		button.Clicked += (_, _) => action();
		layout.Add(button);
	}

	void Reset()
	{
		_pressAction = "";
		if (!_buttons.Contains(_button))
			_buttons.Insert(0, _button);
		_button.IsEnabled = true;
		_button.Padding = new Thickness(12, 8, 20, 16);
		_button.BorderWidth = 2;
		_button.BorderColor = Colors.Purple;
		_button.CornerRadius = 12;
		_button.Background = new SolidColorBrush(Colors.LightGray);
		var presentation = new Grid
		{
			ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
			BackgroundColor = Colors.White
		};
		presentation.Add(new Label { Text = "Nested label", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center });
		presentation.Add(new Image { Source = "dotnet_bot.png", WidthRequest = 40, HeightRequest = 40 }, 1);
		_button.Content = presentation;
		_commands = _clicks = _pressed = _released = 0;
		_order = "";
		UpdateStatus();
	}

	void UpdateStatus() => _status.Text =
		$"Commands={_commands}; Clicks={_clicks}; Pressed={_pressed}; Released={_released}; IsPressed={_button.IsPressed}; Order={_order}";
}
