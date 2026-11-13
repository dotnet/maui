namespace Maui.Controls.Sample;

public partial class TabbedPageOptionsPage : ContentPage
{
	private TabbedPageViewModel _viewModel;

	public TabbedPageOptionsPage(TabbedPageViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private void ApplyButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopModalAsync();
	}

	private void BarBackgroundBrushButton_Clicked(object sender, EventArgs e)
	{
		var button = (Button)sender;
		switch (button.Text)
		{
			case "Gradient":
				_viewModel.BarBackground = new LinearGradientBrush
				{
					StartPoint = new Point(0, 0),
					EndPoint = new Point(1, 1),
					GradientStops = new GradientStopCollection
					{
						new GradientStop { Color = Colors.Red, Offset = 0.0f },
						new GradientStop { Color = Colors.Blue, Offset = 1.0f }
					}
				};
				break;
			case "Solid":
				_viewModel.BarBackground = new SolidColorBrush(Colors.Blue);
				break;
		}
	}

	private void BarTextColorButton_Clicked(object sender, EventArgs e)
	{
		var button = (Button)sender;
		switch (button.Text)
		{
			case "Black":
				_viewModel.BarTextColor = Colors.Black;
				break;
			case "Green":
				_viewModel.BarTextColor = Colors.Green;
				break;
		}
	}

	private void SelectedTabColorButton_Clicked(object sender, EventArgs e)
	{
		var button = (Button)sender;
		switch (button.Text)
		{
			case "Orange":
				_viewModel.SelectedTabColor = Colors.Orange;
				break;
			case "Purple":
				_viewModel.SelectedTabColor = Colors.Purple;
				break;
		}
	}

	private void UnselectedTabColorButton_Clicked(object sender, EventArgs e)
	{
		var button = (Button)sender;
		switch (button.Text)
		{
			case "LightGray":
				_viewModel.UnselectedTabColor = Colors.LightGray;
				break;
			case "DarkGray":
				_viewModel.UnselectedTabColor = Colors.DarkGray;
				break;
		}
	}

	private void OnIsEnabledChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton != null && radioButton.IsChecked)
		{
			_viewModel.IsEnabled = false;
		}
	}

	private void OnFlowDirectionChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton != null && radioButton.IsChecked)
		{
			_viewModel.FlowDirection = radioButton.Content.ToString() == "LeftToRight" ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
		}
	}

	private void ItemsSourceOne_Clicked(object sender, EventArgs e)
	{
		_viewModel.ItemsSource = _viewModel.ItemsSourceOne;
	}

	private void ItemsSourceTwo_Clicked(object sender, EventArgs e)
	{
		_viewModel.ItemsSource = _viewModel.ItemsSourceTwo;
	}

	private void OnTemplateOneClicked(object sender, EventArgs e)
	{
		_viewModel.ItemTemplate = new DataTemplate(() =>
		{
			var label = new Label
			{
				FontAttributes = FontAttributes.Bold,
				FontSize = 18,
				HorizontalOptions = LayoutOptions.Center
			};
			label.SetBinding(Label.AutomationIdProperty, "Id");
			label.SetBinding(Label.TextProperty, "Name");

			var image = new Image
			{
				HorizontalOptions = LayoutOptions.Center,
				WidthRequest = 200,
				HeightRequest = 200
			};
			image.SetBinding(Image.SourceProperty, "ImageUrl");

			var contentPage = new ContentPage
			{
				AutomationId = "ContentPageOne",
				IconImageSource = "bank.png",
				Content = new StackLayout
				{
					Padding = new Thickness(5, 25),
					Children =
					{
						new Label
						{
							Text = "Template One",
							FontAttributes = FontAttributes.Bold,
							FontSize = 18,
							HorizontalOptions = LayoutOptions.Center
						},
						label,
						image
					}
				}
			};
			contentPage.SetBinding(ContentPage.TitleProperty, new Binding("Name"));
			return contentPage;
		});
	}

	private void OnTemplateTwoClicked(object sender, EventArgs e)
	{
		_viewModel.ItemTemplate = new DataTemplate(() =>
		{
			var image = new Image
			{
				HorizontalOptions = LayoutOptions.Center,
				WidthRequest = 140,
				HeightRequest = 140
			};
			image.SetBinding(Image.SourceProperty, "ImageUrl");

			var label = new Label
			{
				FontAttributes = FontAttributes.Bold,
				FontSize = 18,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
			label.SetBinding(Label.AutomationIdProperty, "Id");
			label.SetBinding(Label.TextProperty, "Name");

			var addButton = new Button
			{
				AutomationId = "AddTabButton",
				Text = "Add Tab",
				BackgroundColor = Colors.LightGreen,
				HeightRequest = 50
			};
			addButton.Clicked += (s, e) =>
			{
				if (((Button)s).BindingContext is TabbedPageItemSource item)
				{
					var count = _viewModel.ItemsSource.Count + 1;

					_viewModel.ItemsSource.Add(new TabbedPageItemSource
					{
						Name = $"Tab {count}",
						Id = $"Tab{count}Label",
						ImageUrl = "dotnet_bot.png"
					});
				}
			};

			var removeButton = new Button
			{
				AutomationId = "RemoveTabButton",
				Text = "Remove Tab",
				BackgroundColor = Colors.Tomato,
				HeightRequest = 50
			};
			removeButton.Clicked += (s, e) =>
			{
				if (((Button)s).BindingContext is TabbedPageItemSource item)
				{
					if (_viewModel.ItemsSource.Any())
						_viewModel.ItemsSource.RemoveAt(_viewModel.ItemsSource.Count - 1);
				}
			};

			var insertTabAtButton = new Button
			{
				AutomationId = "InsertTabAtButton",
				Text = "Insert Tab",
				BackgroundColor = Colors.LightBlue,
				HeightRequest = 50
			};
			insertTabAtButton.Clicked += (s, e) =>
			{
				if (((Button)s).BindingContext is TabbedPageItemSource item)
				{
					var count = _viewModel.ItemsSource.Count + 1;

					_viewModel.ItemsSource.Insert(1, new TabbedPageItemSource
					{
						Name = $"NEW TAB",
						Id = "NewTabLabel",
						ImageUrl = "dotnet_bot.png"
					});
				}
			};

			var verticalStack = new VerticalStackLayout
			{
				Spacing = 10,
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					 new HorizontalStackLayout
					{
						Spacing = 1,
						Children =
						{
							addButton,
							removeButton
						}
					},
					insertTabAtButton
				}
			};

			var contentPage = new ContentPage
			{
				AutomationId = "ContentPageTwo",
				IconImageSource = "fruitsicon.png",
				Content = new VerticalStackLayout
				{
					Padding = new Thickness(5, 25),
					Spacing = 40,
					HorizontalOptions = LayoutOptions.Center,
					Children =
					{
						new Label
						{
							Text = "Template Two",
							FontAttributes = FontAttributes.Bold,
							FontSize = 18,
							HorizontalOptions = LayoutOptions.Center
						},

						new HorizontalStackLayout
						{
							VerticalOptions = LayoutOptions.Center,
							Spacing=1,
							Children =
							{
								image,
								label,
								verticalStack
							}
						}
					}
				}
			};
			contentPage.SetBinding(ContentPage.TitleProperty, new Binding("Name"));

			var changeIconButton = new Button
			{
				AutomationId = "ChangeIconButton",
				Text = "Change Icon Image",
				BackgroundColor = Colors.Orange,
				HeightRequest = 50
			};
			changeIconButton.Clicked += (s, e) =>
			{
				contentPage.IconImageSource = "star_flyout.png";
			};

			verticalStack.Children.Add(changeIconButton);
			return contentPage;
		});
	}

	private void SelectItemButton_Clicked(object sender, EventArgs e)
	{
		var index = int.Parse(selectedItemEntry.Text);
		if (index > 0 && index <= _viewModel.ItemsSource.Count)
		{
			_viewModel.SelectedItem = _viewModel.ItemsSource[index - 1];
		}
	}
}
