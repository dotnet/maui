#nullable enable

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 37691, "Page scrolling behavior upon keyboard hide is broken", PlatformAffected.iOS)]
public class Issue37691 : ContentPage
{
	readonly Label _layoutStatus;
	readonly Label _bottomMarker;

	public Issue37691()
	{
		Title = "Keyboard modal layout restoration";
		BackgroundColor = Colors.DarkBlue;

		_layoutStatus = new Label
		{
			AutomationId = "LayoutStatus",
			TextColor = Colors.White,
			HorizontalOptions = LayoutOptions.Center
		};

		_bottomMarker = new Label
		{
			AutomationId = "BottomMarker",
			Text = "BOTTOM OF UNDERLYING PAGE",
			BackgroundColor = Colors.Lime,
			TextColor = Colors.Black,
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalTextAlignment = TextAlignment.Center,
			HeightRequest = 48
		};

		var showModalButton = new Button
		{
			AutomationId = "ShowModal",
			Text = "Show modal",
			HorizontalOptions = LayoutOptions.Center
		};
		showModalButton.Clicked += OnShowModal;

		var grid = new Grid
		{
			SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.SoftInput),
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star),
				new RowDefinition(GridLength.Auto)
			}
		};
		grid.Add(_layoutStatus);
		grid.Add(showModalButton, row: 1);
		grid.Add(_bottomMarker, row: 2);

		Content = grid;
		SizeChanged += (_, _) => UpdateLayoutStatus();
		_bottomMarker.SizeChanged += (_, _) => UpdateLayoutStatus();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await Task.Delay(300);
		UpdateLayoutStatus();
	}

	async void OnShowModal(object? sender, EventArgs e)
	{
		await Navigation.PushModalAsync(new KeyboardModalPage());
	}

	void UpdateLayoutStatus()
	{
		_layoutStatus.Text = $"Page height: {Height:F1}; bottom: {_bottomMarker.Y + _bottomMarker.Height:F1}";
	}

	sealed class KeyboardModalPage : ContentPage
	{
		readonly Entry _entry;

		public KeyboardModalPage()
		{
			BackgroundColor = Colors.LightGray;

			_entry = new Entry
			{
				AutomationId = "ModalEntry",
				Placeholder = "Keyboard must remain visible"
			};

			var dismissButton = new Button
			{
				AutomationId = "DismissModal",
				Text = "Dismiss modal"
			};
			dismissButton.Clicked += async (_, _) => await Navigation.PopModalAsync();

			Content = new VerticalStackLayout
			{
				Padding = 30,
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						Text = "Dismiss this modal while the keyboard is visible.",
						TextColor = Colors.Black
					},
					_entry,
					dismissButton
				}
			};
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await Task.Delay(300);
			_entry.Focus();
		}
	}
}
