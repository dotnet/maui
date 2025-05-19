namespace Maui.Controls.Sample;

public partial class CarouselViewOptionsPage : ContentPage
{
	private CarouselViewViewModel _viewModel;

	public CarouselViewOptionsPage(CarouselViewViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private async void ApplyButton_Clicked(object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}

	private void OnEmptyViewChanged(object sender, CheckedChangedEventArgs e)
	{
		if (EmptyViewNone.IsChecked)
		{
			_viewModel.EmptyView = null; // No empty view
		}
		else if (EmptyViewString.IsChecked)
		{
			_viewModel.EmptyView = "No items available"; // String
		}
		else if (EmptyViewCustomView.IsChecked)
		{
			_viewModel.EmptyView = new StackLayout
			{
				Children =
					{
						new Label
						{
							Text = "No items available(Custom View)",
							FontSize = 18,
							HorizontalOptions = LayoutOptions.Center,
							VerticalOptions = LayoutOptions.Center,
							TextColor = Colors.Red
						},
						new Image
						{
							Source = "dotnet_bot.png",
							HeightRequest = 120,
							WidthRequest = 120,
							HorizontalOptions = LayoutOptions.Center
						}
					}
			};
			_viewModel.EmptyViewTemplate = null;

		}

		else if (EmptyViewDataTemplate.IsChecked)
		{
			_viewModel.EmptyView = null;
			_viewModel.EmptyViewTemplate = new DataTemplate(() =>
			{
				return new StackLayout
				{
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					Children =
					{
					new Label
					{
						Text = "No items available (DataTemplate)",
						TextColor = Colors.Red,
						FontSize = 16,
						HorizontalOptions = LayoutOptions.Center
					},
					new Image
					{
						Source = "dotnet_bot.png",
						HeightRequest = 100,
						WidthRequest = 100,
						HorizontalOptions = LayoutOptions.Center
					}
					}
				};
			});
		}
	}


	private void OnItemTemplateChanged(object sender, CheckedChangedEventArgs e)
	{
		if (ItemTemplateNone.IsChecked)
		{
			_viewModel.ItemTemplate = null;
		}
		else if (ItemTemplateGrid.IsChecked)
		{
			_viewModel.ItemTemplate = new DataTemplate(() =>
			{
				Grid grid = new Grid
				{
					BackgroundColor = Colors.LightGray,
					HeightRequest = 200,
					WidthRequest = 300,
					Margin = new Thickness(10)
				};

				var label = new Label
				{
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					FontSize = 24,
					TextColor = Colors.DarkBlue
				};

				// Bind the text with a suffix "(Grid Template)"
				label.SetBinding(Label.TextProperty, new Binding(".")
				{
					StringFormat = "{0} (Grid Template)"
				});

				grid.Children.Add(label);
				return grid;
			});
		}
		else if (ItemTemplateCustomView.IsChecked)
		{
			_viewModel.ItemTemplate = new DataTemplate(() =>
			{
				var frame = new Frame
				{
					BorderColor = Colors.Blue,
					CornerRadius = 10,
					Margin = new Thickness(20),
					HeightRequest = 300,
					WidthRequest = 300,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				};

				var stackLayout = new StackLayout
				{
					Spacing = 10
				};

				var label = new Label
				{
					FontSize = 24,
					TextColor = Colors.DarkBlue,
					HorizontalOptions = LayoutOptions.Center
				};

				// Bind the text with a suffix "(Custom View Template)"
				label.SetBinding(Label.TextProperty, new Binding(".")
				{
					StringFormat = "{0} (Image View)"
				});

				var image = new Image
				{
					Source = "dotnet_bot.png",
					HeightRequest = 150,
					WidthRequest = 150,
					HorizontalOptions = LayoutOptions.Center
				};

				stackLayout.Children.Add(label);
				stackLayout.Children.Add(image);
				frame.Content = stackLayout;

				return frame;
			});
		}
	}

	private void OnItemsSourceChanged(object sender, CheckedChangedEventArgs e)
	{
		if (ItemsSourceNone.IsChecked)
		{
			_viewModel.ItemsSourceType = CarouselItemsSourceType.None;
		}
		else if (ObservableCollection.IsChecked)
		{
			_viewModel.ItemsSourceType = CarouselItemsSourceType.ObservableCollectionT;
		}
	}

	private void OnItemsLayoutChanged(object sender, CheckedChangedEventArgs e)
	{
		if (ItemsLayoutVertical.IsChecked)
		{
			_viewModel.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical);
		}
		else if (ItemsLayoutHorizontal.IsChecked)
		{
			_viewModel.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);
		}
	}

	private void OnItemsUpdatingScrollModeChanged(object sender, CheckedChangedEventArgs e)
	{
		if (KeepItemsInView.IsChecked)
		{
			_viewModel.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepItemsInView;
		}
		else if (KeepScrollOffset.IsChecked)
		{
			_viewModel.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;
		}
		else if (KeepLastItemInView.IsChecked)
		{
			_viewModel.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView;
		}
	}

	private void OnLoopLabelTapped(object sender, EventArgs e)
	{
		Loop.IsChecked = true;
	}

	private void OnSwipeLabelTapped(object sender, EventArgs e)
	{
		IsSwipeEnabled.IsChecked = !IsSwipeEnabled.IsChecked;
	}

	private void OnPeekAreaInsetsEntryCompleted(object sender, EventArgs e)
	{
		if (sender is Entry entry)
		{
			if (double.TryParse(entry.Text, out double value))
			{
				value = Math.Clamp(value, 0, 500);
				_viewModel.PeekAreaInsets = new Thickness(value);
				entry.Text = value.ToString();
			}
		}
	}

	private void OnPositionEntryCompleted(object sender, EventArgs e)
	{
		if (sender is Entry entry)
		{
			if (int.TryParse(entry.Text, out int value))
			{
				value = Math.Clamp(value, 0, 4);
				_viewModel.Position = value;
				entry.Text = value.ToString();
			}
		}
	}

	private void OnFlowDirectionChanged(object sender, EventArgs e)
	{
		_viewModel.FlowDirection = FlowDirectionLTR.IsChecked ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
	}
}