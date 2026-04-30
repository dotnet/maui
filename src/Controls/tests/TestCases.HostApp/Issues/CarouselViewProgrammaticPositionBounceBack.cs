using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(
		IssueTracker.Github,
		21480,
		"Programmatic Position/CurrentItem set bounces back; ItemsSource reset to 0",
		PlatformAffected.Android | PlatformAffected.iOS
	)]
	public class CarouselViewProgrammaticPositionBounceBack : ContentPage
	{
		const string CarouselId = "carousel";
		const string PositionLabelId = "positionLabel";
		const string PositionEventCountId = "positionEventCount";
		const string CurrentItemEventCountId = "currentItemEventCount";
		const string SetPositionBtnId = "setPositionBtn";
		const string ReloadBtnId = "reloadBtn";

		readonly CarouselView _carousel;
		readonly Label _positionLabel;
		readonly Label _positionEventCount;
		readonly Label _currentItemEventCount;
		ObservableCollection<string> _items;
		int _positionChangedCount;
		int _currentItemChangedCount;

		public CarouselViewProgrammaticPositionBounceBack()
		{
			_items = new ObservableCollection<string>(
				Enumerable.Range(0, 5).Select(i => $"Item {i}")
			);

			_carousel = new CarouselView
			{
				AutomationId = CarouselId,
				ItemsSource = _items,
				HeightRequest = 200,
				BackgroundColor = Colors.LightYellow,
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label
					{
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						FontSize = 24,
					};
					label.SetBinding(Label.TextProperty, ".");
					return label;
				}),
			};

			_positionLabel = new Label { Text = "Pos:0", AutomationId = PositionLabelId };
			_positionEventCount = new Label
			{
				Text = "PosChanged:0",
				AutomationId = PositionEventCountId,
			};
			_currentItemEventCount = new Label
			{
				Text = "ItemChanged:0",
				AutomationId = CurrentItemEventCountId,
			};

			_carousel.PositionChanged += (s, e) =>
			{
				_positionChangedCount++;
				_positionEventCount.Text = $"PosChanged:{_positionChangedCount}";
				_positionLabel.Text = $"Pos:{e.CurrentPosition}";
			};

			_carousel.CurrentItemChanged += (s, e) =>
			{
				_currentItemChangedCount++;
				_currentItemEventCount.Text = $"ItemChanged:{_currentItemChangedCount}";
			};

			var setPositionBtn = new Button
			{
				Text = "Set Position to 3",
				AutomationId = SetPositionBtnId,
			};
			setPositionBtn.Clicked += (s, e) =>
			{
				_positionChangedCount = 0;
				_currentItemChangedCount = 0;
				_positionEventCount.Text = "PosChanged:0";
				_currentItemEventCount.Text = "ItemChanged:0";
				_carousel.Position = 3;
			};

			var reloadBtn = new Button { Text = "Reload + SetPos2", AutomationId = ReloadBtnId };
			reloadBtn.Clicked += (s, e) =>
			{
				_positionChangedCount = 0;
				_currentItemChangedCount = 0;
				_positionEventCount.Text = "PosChanged:0";
				_currentItemEventCount.Text = "ItemChanged:0";
				_items = new ObservableCollection<string>(
					Enumerable.Range(0, 5).Select(i => $"Item {i}")
				);
				_carousel.ItemsSource = _items;
				_carousel.Position = 2;
			};

			Content = new VerticalStackLayout
			{
				Spacing = 4,
				Padding = 8,
				Children =
				{
					new Label
					{
						Text =
							"Repro: programmatic Position must not bounce back; ItemsSource reset must honor explicit Position.",
					},
					_positionLabel,
					_positionEventCount,
					_currentItemEventCount,
					setPositionBtn,
					reloadBtn,
					_carousel,
				},
			};
		}
	}
}
