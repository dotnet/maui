using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35643, "CurrentItem is updated incorrectly on Android when the CarouselView is bound to an ObservableCollection with Loop = false", PlatformAffected.Android)]
public class Issue35643 : ContentPage
{
	readonly Issue35643ViewModel _viewModel;

	public Issue35643()
	{
		_viewModel = new Issue35643ViewModel
		{
			Items = new ObservableCollection<string> { "0", "1", "2" },
			CurrentItem = "2",
			LoopItems = new ObservableCollection<string> { "A", "B", "C" },
			LoopCurrentItem = "C"
		};

		// ── Section 1: Replace current item (Loop=false) ────────────────────────
		var currentItemLabel = new Label
		{
			AutomationId = "CurrentItemLabel",
			FontSize = 24,
			HorizontalTextAlignment = TextAlignment.Center
		};
		currentItemLabel.SetBinding(Label.TextProperty, new Binding(nameof(Issue35643ViewModel.CurrentItem)));

		var carousel = new CarouselView
		{
			Loop = false,
			AutomationId = "CarouselView",
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center,
					FontSize = 20
				};
				label.SetBinding(Label.TextProperty, ".");
				return new Border { Content = label, BackgroundColor = Colors.LightBlue };
			})
		};
		carousel.SetBinding(CarouselView.ItemsSourceProperty, new Binding(nameof(Issue35643ViewModel.Items)));
		carousel.SetBinding(CarouselView.CurrentItemProperty, new Binding(nameof(Issue35643ViewModel.CurrentItem)));

		var positionLabel = new Label
		{
			AutomationId = "PositionLabel",
			FontSize = 18,
			HorizontalTextAlignment = TextAlignment.Center
		};
		positionLabel.SetBinding(Label.TextProperty, new Binding("Position", source: carousel));

		var updateButton = new Button
		{
			Text = "Replace item 2 and update CurrentItem",
			AutomationId = "UpdateButton"
		};
		updateButton.Clicked += (s, e) =>
		{
			_viewModel.Items[2] = "2b";
			_viewModel.CurrentItem = "2b";
		};

		// ── Section 2: Replace current item (Loop=true) ────────────────────────
		// Initialized at "C" (position 2) to avoid the iOS/Mac virtual-adapter timing race
		// where Loop=true renders at Row 0 of the virtual adapter (maps to index itemCount-1=2).
		var loopCurrentItemLabel = new Label
		{
			AutomationId = "LoopCurrentItemLabel",
			FontSize = 24,
			HorizontalTextAlignment = TextAlignment.Center
		};
		loopCurrentItemLabel.SetBinding(Label.TextProperty, new Binding(nameof(Issue35643ViewModel.LoopCurrentItem)));

		var loopCarousel = new CarouselView
		{
			Loop = true,
			AutomationId = "LoopCarouselView",
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center,
					FontSize = 20
				};
				label.SetBinding(Label.TextProperty, ".");
				return new Border { Content = label, BackgroundColor = Colors.LightCoral };
			})
		};
		loopCarousel.SetBinding(CarouselView.ItemsSourceProperty, new Binding(nameof(Issue35643ViewModel.LoopItems)));
		loopCarousel.SetBinding(CarouselView.CurrentItemProperty, new Binding(nameof(Issue35643ViewModel.LoopCurrentItem)));

		var loopPositionLabel = new Label
		{
			AutomationId = "LoopPositionLabel",
			FontSize = 18,
			HorizontalTextAlignment = TextAlignment.Center
		};
		loopPositionLabel.SetBinding(Label.TextProperty, new Binding("Position", source: loopCarousel));

		var loopReplaceButton = new Button
		{
			Text = "Replace current item in loop carousel",
			AutomationId = "LoopReplaceButton"
		};
		loopReplaceButton.Clicked += (s, e) =>
		{
			_viewModel.LoopItems[2] = "C2";
			_viewModel.LoopCurrentItem = "C2";
		};

		BindingContext = _viewModel;

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(10),
				Spacing = 10,
				Children =
				{
					new Label { Text = "── Loop=false: Replace current item ──", FontAttributes = FontAttributes.Bold },
					new Label { Text = "Current Item:" },
					currentItemLabel,
					positionLabel,
					new ContentView { Content = carousel, HeightRequest = 200 },
					updateButton,
					new Label { Text = "── Loop=true: Replace current item ──", FontAttributes = FontAttributes.Bold },
					new Label { Text = "Loop Current Item:" },
					loopCurrentItemLabel,
					loopPositionLabel,
					new ContentView { Content = loopCarousel, HeightRequest = 200 },
					loopReplaceButton,
				}
			}
		};
	}
}

public class Issue35643ViewModel : INotifyPropertyChanged
{
	ObservableCollection<string> _items;
	string _currentItem;
	ObservableCollection<string> _loopItems;
	string _loopCurrentItem;

	public ObservableCollection<string> Items
	{
		get => _items;
		set => SetProperty(ref _items, value);
	}

	public string CurrentItem
	{
		get => _currentItem;
		set => SetProperty(ref _currentItem, value);
	}

	public ObservableCollection<string> LoopItems
	{
		get => _loopItems;
		set => SetProperty(ref _loopItems, value);
	}

	public string LoopCurrentItem
	{
		get => _loopCurrentItem;
		set => SetProperty(ref _loopCurrentItem, value);
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

	protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
	{
		if (EqualityComparer<T>.Default.Equals(backingStore, value))
		{
			return false;
		}
		backingStore = value;
		OnPropertyChanged(propertyName);
		return true;
	}
}
