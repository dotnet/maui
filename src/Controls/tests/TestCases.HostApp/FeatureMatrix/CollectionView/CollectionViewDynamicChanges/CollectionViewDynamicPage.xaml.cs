namespace Maui.Controls.Sample;

public partial class CollectionViewDynamicPage : ContentPage
{
	private CollectionViewViewModel _viewModel;

	public CollectionViewDynamicPage()
	{
		InitializeComponent();
		_viewModel = new CollectionViewViewModel();
		_viewModel.ItemsSourceType = ItemsSourceType.ObservableCollection5T;
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new CollectionViewViewModel();
		_viewModel.ItemsSourceType = ItemsSourceType.ObservableCollection5T;
		await Navigation.PushAsync(new CollectionViewDynamicOptionsPage(_viewModel));
	}

	private bool _isHeaderString1 = true;
	private bool _isFooterString1 = true;
	private bool _isFooterGrid1 = true;
	private bool _isHeaderGrid1 = true;
	private bool _isHeaderTemplate1 = true;
	private bool _isFooterTemplate1 = true;
	private bool _isGroupFooterTemplate1 = true;
	private bool _isGroupHeaderTemplate1 = true;
	private bool _isEmptyViewGrid1 = true;
	private bool _isEmptyViewString1 = true;

	private bool _isEmptyViewTemplate1 = true;
	private bool _isItemTemplate1 = true;

	private void OnHeaderStringButtonClicked(object sender, EventArgs e)
	{
		if (_viewModel.IsHeaderStringSelected) // If HeaderString is active
		{
			if (_isHeaderString1)
			{
				_viewModel.Header = "Header String1";
			}
			else
			{
				_viewModel.Header = "Header String2";
			}
			_isHeaderString1 = !_isHeaderString1;
		}
	}

	private void OnFooterStringButtonClicked(object sender, EventArgs e)
	{
		if (_viewModel.IsFooterStringSelected) // If FooterString is active
		{
			if (_isFooterString1)
			{
				_viewModel.Footer = "Footer String1";
			}
			else
			{
				_viewModel.Footer = "Footer String2";
			}
			_isFooterString1 = !_isFooterString1;
		}
	}

	private void OnHeaderGridButtonClicked(object sender, EventArgs e)
	{
		if (_viewModel.IsHeaderGridSelected)
		{
			if (_isHeaderGrid1)
			{
				Grid grid = new Grid
				{
					BackgroundColor = Colors.LightGray,
					Padding = new Thickness(10)
				};

				grid.Children.Add(new Label
				{
					Text = "Header Grid1",
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					TextColor = Colors.Blue,
					FontSize = 18,
					FontAttributes = FontAttributes.Bold
				});

				_viewModel.Header = grid;
			}
			else
			{
				Grid grid = new Grid
				{
					BackgroundColor = Colors.LightYellow,
					Padding = new Thickness(20)
				};

				grid.Children.Add(new Label
				{
					Text = "Header Grid2",
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					TextColor = Colors.Red,
					FontSize = 20,
					HeightRequest = 150,
				});

				_viewModel.Header = grid;
			}
			_isHeaderGrid1 = !_isHeaderGrid1;
		}
	}


	private void OnFooterGridButtonClicked(object sender, EventArgs e)
	{
		if (_viewModel.IsFooterGridSelected)
		{
			if (_isFooterGrid1)
			{
				Grid grid = new Grid
				{
					BackgroundColor = Colors.LightGray,
					Padding = new Thickness(10)
				};

				grid.Children.Add(new Label
				{
					Text = "Footer Grid1",
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					TextColor = Colors.Blue,
					FontSize = 18,
					FontAttributes = FontAttributes.Bold
				});

				_viewModel.Footer = grid;
			}
			else
			{
				Grid grid = new Grid
				{
					BackgroundColor = Colors.LightYellow,
					Padding = new Thickness(20)
				};

				grid.Children.Add(new Label
				{
					Text = "Footer Grid2",
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					TextColor = Colors.Red,
					FontSize = 20,
					HeightRequest = 150,
				});

				_viewModel.Footer = grid;
			}
			_isFooterGrid1 = !_isFooterGrid1;
		}
	}


	private void OnHeaderTemplateButtonClicked(object sender, EventArgs e)
	{
		if (_viewModel.IsHeaderTemplateViewSelected)
		{
			if (_isHeaderTemplate1)
			{
				_viewModel.HeaderTemplate = new DataTemplate(() =>
				{
					var grid = new Grid
					{
						BackgroundColor = Colors.LightGray,
						Padding = new Thickness(10)
					};
					grid.Children.Add(new Label
					{
						Text = "Header Template1",
						FontSize = 18,
						FontAttributes = FontAttributes.Bold,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						TextColor = Colors.Blue,
					});
					return grid;
				});
			}
			else
			{
				_viewModel.HeaderTemplate = new DataTemplate(() =>
				{
					var grid = new Grid
					{
						BackgroundColor = Colors.LightYellow,
						Padding = new Thickness(20)
					};
					grid.Children.Add(new Label
					{
						Text = "Header Template2",
						FontSize = 20,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						TextColor = Colors.Red,
						HeightRequest = 150,
					});
					return grid;
				});
			}

			_isHeaderTemplate1 = !_isHeaderTemplate1;
		}
	}

	private void OnFooterTemplateButtonClicked(object sender, EventArgs e)
	{
		if (_viewModel.IsFooterTemplateViewSelected)
		{
			if (_isFooterTemplate1)
			{
				_viewModel.FooterTemplate = new DataTemplate(() =>
				{
					var grid = new Grid
					{
						BackgroundColor = Colors.LightGray,
						Padding = new Thickness(10)
					};
					grid.Children.Add(new Label
					{
						Text = "Footer Template1",
						FontSize = 18,
						FontAttributes = FontAttributes.Bold,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						TextColor = Colors.Blue,
					});
					return grid;
				});
			}
			else
			{
				_viewModel.FooterTemplate = new DataTemplate(() =>
				{
					var grid = new Grid
					{
						BackgroundColor = Colors.LightYellow,
						Padding = new Thickness(20)
					};
					grid.Children.Add(new Label
					{
						Text = "Footer Template2",
						FontSize = 20,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						TextColor = Colors.Red,
						HeightRequest = 150,
					});
					return grid;
				});
			}

			_isFooterTemplate1 = !_isFooterTemplate1;
		}
	}


	private void OnGroupHeaderTemplateButtonClicked(object sender, EventArgs e)
	{
		if (_viewModel.IsGroupHeaderTemplateViewSelected)
		{
			if (_isGroupHeaderTemplate1)
			{
				_viewModel.GroupHeaderTemplate = new DataTemplate(() =>
				{
					var grid = new Grid
					{
						BackgroundColor = Colors.LightGray,
						Padding = new Thickness(10)
					};
					grid.Children.Add(new Label
					{
						Text = "GroupHeaderTemplate1",
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						TextColor = Colors.Green,
						FontSize = 18,
						FontAttributes = FontAttributes.Bold,
					});
					return grid;
				});
			}
			else
			{
				_viewModel.GroupHeaderTemplate = new DataTemplate(() =>
				{
					var grid = new Grid
					{
						BackgroundColor = Colors.LightYellow,
						Padding = new Thickness(20)
					};
					grid.Children.Add(new Label
					{
						Text = "GroupHeaderTemplate2",
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						TextColor = Colors.Red,
						FontSize = 20,
					});
					return grid;
				});
			}
			_isGroupHeaderTemplate1 = !_isGroupHeaderTemplate1;
		}
	}



	private void OnGroupFooterTemplateButtonClicked(object sender, EventArgs e)
	{
		if (_viewModel.IsGroupFooterTemplateViewSelected)
		{
			if (_isGroupFooterTemplate1)
			{
				_viewModel.GroupFooterTemplate = new DataTemplate(() =>
				{
					var grid = new Grid
					{
						BackgroundColor = Colors.LightGray,
						Padding = new Thickness(10)
					};
					grid.Children.Add(new Label
					{
						Text = "GroupFooterTemplate1",
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						TextColor = Colors.Green,
						FontSize = 18,
						FontAttributes = FontAttributes.Bold,
					});
					return grid;
				});
			}
			else
			{
				_viewModel.GroupFooterTemplate = new DataTemplate(() =>
				{
					var grid = new Grid
					{
						BackgroundColor = Colors.LightYellow,
						Padding = new Thickness(20)
					};
					grid.Children.Add(new Label
					{
						Text = "GroupFooterTemplate2",
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						TextColor = Colors.Red,
						FontSize = 20,
					});
					return grid;
				});
			}

			_isGroupFooterTemplate1 = !_isGroupFooterTemplate1;
		}
	}

	private void OnEmptyViewStringButtonClicked(object sender, EventArgs e)
	{
		if (_viewModel.IsEmptyViewStringSelected)
		{
			if (_isEmptyViewString1)
			{
				_viewModel.EmptyView = "EmptyView String1: No items available";
			}
			else
			{
				_viewModel.EmptyView = "EmptyView String2: No items available";
			}

			_isEmptyViewString1 = !_isEmptyViewString1;
		}
	}



	private void OnEmptyViewGridButtonClicked(object sender, EventArgs e)
	{
		if (_viewModel.IsEmptyViewGridSelected)
		{
			if (_isEmptyViewGrid1)
			{
				Grid grid = new Grid
				{
					BackgroundColor = Colors.LightBlue,
					Padding = new Thickness(10)
				};

				grid.Children.Add(new Label
				{
					Text = "EmptyView Grid1: No items available",
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					TextColor = Colors.Blue,
					FontSize = 18,
					FontAttributes = FontAttributes.Bold
				});

				_viewModel.EmptyView = grid;
			}
			else
			{
				Grid grid = new Grid
				{
					BackgroundColor = Colors.LightYellow,
					Padding = new Thickness(20)
				};

				grid.Children.Add(new Label
				{
					Text = "EmptyView Grid2: No items available",
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					TextColor = Colors.Red,
					FontSize = 14,
				});

				_viewModel.EmptyView = grid;
			}

			_isEmptyViewGrid1 = !_isEmptyViewGrid1;
		}
	}

	private void OnEmptyViewTemplateButtonClicked(object sender, EventArgs e)
	{
		if (_viewModel.IsEmptyViewTemplateSelected)
		{
			if (_isEmptyViewTemplate1)
			{
				_viewModel.EmptyViewTemplate = new DataTemplate(() =>
				{
					Grid grid = new Grid
					{
						BackgroundColor = Colors.LightGray,
						Padding = new Thickness(10)
					};

					grid.Children.Add(new Label
					{
						Text = "EmptyViewTemplate1: No items available",
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						TextColor = Colors.Green,
						FontSize = 18,
						FontAttributes = FontAttributes.Bold
					});

					return grid;
				});
			}
			else
			{
				_viewModel.EmptyViewTemplate = new DataTemplate(() =>
				{
					Grid grid = new Grid
					{
						BackgroundColor = Colors.LightYellow,
						Padding = new Thickness(20)
					};

					grid.Children.Add(new Label
					{
						Text = "EmptyViewTemplate2: No items available",
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						TextColor = Colors.Red,
						FontSize = 14,
					});

					return grid;
				});
			}

			_isEmptyViewTemplate1 = !_isEmptyViewTemplate1;
		}
	}

	private void OnItemTemplateButtonClicked(object sender, EventArgs e)
	{
		if (_viewModel.IsItemTemplateSelected)
		{
			if (_isItemTemplate1)
			{
				_viewModel.ItemTemplate = new DataTemplate(() =>
				{
					Grid grid = new Grid
					{
						BackgroundColor = Colors.LightGray,
						Padding = new Thickness(10)
					};

					Label label = new Label
					{
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						TextColor = Colors.Blue,
						FontSize = 18,
					};

					label.SetBinding(Label.TextProperty, new Binding("Caption", stringFormat: "Template 1 - {0}"));

					grid.Children.Add(label);

					return grid;
				});
			}
			else
			{
				_viewModel.ItemTemplate = new DataTemplate(() =>
				{
					Grid grid = new Grid
					{
						BackgroundColor = Colors.LightYellow,
						Padding = new Thickness(20)
					};

					Label label = new Label
					{
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						TextColor = Colors.Red,
						FontSize = 14,
					};

					label.SetBinding(Label.TextProperty, new Binding("Caption", stringFormat: "Template 2 - {0}"));

					grid.Children.Add(label);

					return grid;
				});
			}
			_isItemTemplate1 = !_isItemTemplate1;
		}
	}
}