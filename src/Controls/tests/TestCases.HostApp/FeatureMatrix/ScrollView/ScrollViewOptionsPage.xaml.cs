namespace Maui.Controls.Sample;

public partial class ScrollViewOptionsPage : ContentPage
{
	private readonly ScrollViewViewModel _viewModel;

	public ScrollViewOptionsPage(ScrollViewViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private void ApplyButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}


	private void OnContentTypeButtonClicked(object sender, EventArgs e)
	{
		if (sender is not Button btn)
			return;
		if (BindingContext is not ScrollViewViewModel vm)
			return;
		switch (btn.Text)
		{
			case "Label":
				vm.Content = new Label
				{
					Text = string.Join(Environment.NewLine, Enumerable.Range(1, 60).Select(i => $"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed euismod, urna eu tincidunt consectetur, nisi nisl aliquam enim, eget facilisis enim nisl nec elit . Sed euismod, urna eu tincidunt consectetur, nisi nisl aliquam enim Eget facilisis enim nisl nec elit Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae. Nullam ac erat at dui laoreet aliquet. Praesent euismod, justo at dictum facilisis, urna erat dictum enim.{i}")),
					FontSize = 16,
					Padding = 10
				};
				break;
			case "Image":
#if WINDOWS || MACCATALYST
                double Imagewidth = 1300;
#else
				double Imagewidth = 1000;
#endif
				vm.Content = new Image
				{
					Source = "dotnet_bot.png",
					HeightRequest = 1000,
					WidthRequest = Imagewidth,
					Aspect = Aspect.AspectFit
				};
				break;

			case "Editor":
				var editorLayout = new VerticalStackLayout();

				editorLayout.Children.Add(new Editor
				{
					IsSpellCheckEnabled = false,
					Text = string.Join(Environment.NewLine, Enumerable.Range(1, 15).Select(i => $"Editor 1: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed euismod, urna eu tincidunt consectetur, nisi nisl aliquam enim, eget facilisis enim nisl nec elit . Sed euismod, urna eu tincidunt consectetur, nisi nisl aliquam enim Eget facilisis enim nisl nec elit Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae. Nullam ac erat at dui laoreet aliquet. Praesent euismod, justo at dictum facilisis, urna erat dictum enim.")),
					HeightRequest = 250,
					AutomationId = "Editor1"
				});

				editorLayout.Children.Add(new Editor
				{
					IsSpellCheckEnabled = false,
					Text = string.Join(Environment.NewLine, Enumerable.Range(1, 15).Select(i => $"Editor 2: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed euismod, urna eu tincidunt consectetur, nisi nisl aliquam enim, eget facilisis enim nisl nec elit . Sed euismod, urna eu tincidunt consectetur, nisi nisl aliquam enim Eget facilisis enim nisl nec elit Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae. Nullam ac erat at dui laoreet aliquet. Praesent euismod, justo at dictum facilisis, urna erat dictum enim.")),
					HeightRequest = 250,
					AutomationId = "Editor2"
				});

				editorLayout.Children.Add(new Editor
				{
					IsSpellCheckEnabled = false,
					Text = string.Join(Environment.NewLine, Enumerable.Range(1, 15).Select(i => $"Editor 3: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed euismod, urna eu tincidunt consectetur, nisi nisl aliquam enim, eget facilisis enim nisl nec elit . Sed euismod, urna eu tincidunt consectetur, nisi nisl aliquam enim Eget facilisis enim nisl nec elit Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae. Nullam ac erat at dui laoreet aliquet. Praesent euismod, justo at dictum facilisis, urna erat dictum enim.")),
					HeightRequest = 250,
					AutomationId = "Editor3"
				});

				editorLayout.Children.Add(new Editor
				{
					IsSpellCheckEnabled = false,
					Text = string.Join(Environment.NewLine, Enumerable.Range(1, 15).Select(i => $"Editor 4: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed euismod, urna eu tincidunt consectetur, nisi nisl aliquam enim, eget facilisis enim nisl nec elit . Sed euismod, urna eu tincidunt consectetur, nisi nisl aliquam enim Eget facilisis enim nisl nec elit Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae. Nullam ac erat at dui laoreet aliquet. Praesent euismod, justo at dictum facilisis, urna erat dictum enim.")),
					HeightRequest = 250,
					AutomationId = "Editor4"
				});

				vm.Content = editorLayout;
				break;
			case "VerticalStackLayout":
				var labelLayout = new VerticalStackLayout();
				string[] fruits = {
					 "Apple", "Banana", "Cherry",
					 "Date", "Elderberry", "Fig",
					 "Grape", "Honeydew","Kiwi",
					 "Lemon", "Mango", "Nectarine",
					 "Orange", "Papaya", "Quince",
					 "Ilama", "Jabuticaba", "Kiwano",
					 "Longan", "Mandarin", "Naranjilla",
					 "Olallieberry", "Persimmon",
					 "Rose Apple", "Soursop",
					 };

				for (int i = 0; i < 50; i++)
				{
					var fruit = fruits[i % fruits.Length];
					labelLayout.Children.Add(new Label { Text = fruit, FontSize = 18, HorizontalOptions = LayoutOptions.Center });
				}
				vm.Content = labelLayout;
				break;

			case "Grid":
				int columns;
#if WINDOWS || MACCATALYST
                columns = 20;
#else
				columns = 6;
#endif
				int rows = 30;

				var grid = new Grid();
				for (int row = 0; row < rows; row++)
					grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
				for (int col = 0; col < columns; col++)
					grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

				int totalItems = rows * columns;

				for (int row = 0; row < rows; row++)
					for (int col = 0; col < columns; col++)
					{
						int index = row * columns + col + 1; // +1 to start from 1
						var cell = new Label
						{
							Text = $"Item {index}",
							FontSize = 16,
							HorizontalOptions = LayoutOptions.Center,
							Padding = new Thickness(8)
						};
						grid.Add(cell, col, row);
					}

				vm.Content = grid;
				break;
			case "AbsoluteLayout":
				double width, height;
#if WINDOWS || MACCATALYST
                width = 1300;
                height = 1000;
#else
				width = 800;
				height = 800;
#endif
				var absolute = new AbsoluteLayout
				{
					WidthRequest = width,
					HeightRequest = height,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				};


				for (int i = 0; i < 10; i++)
				{
					var box = new BoxView
					{
						Color = i % 2 == 0 ? Colors.CornflowerBlue : Colors.Orange,
						WidthRequest = 80,
						HeightRequest = 80
					};
					AbsoluteLayout.SetLayoutBounds(box, new Rect(30 + i * 70, 30 + i * 70, 80, 80));
					absolute.Children.Add(box);
				}

				for (int i = 0; i < 10; i++)
				{
					var box = new BoxView
					{
						Color = i % 2 == 0 ? Colors.MediumPurple : Colors.ForestGreen,
						WidthRequest = 80,
						HeightRequest = 80
					};
					AbsoluteLayout.SetLayoutBounds(box, new Rect(30 + (9 - i) * 70, 30 + i * 70, 80, 80));
					absolute.Children.Add(box);
				}

				vm.Content = absolute;
				break;

			case "HorizontalStackLayout":
				var hStack = new HorizontalStackLayout();
				string[] fruitsArray = {
					 "Apple", "Banana", "Cherry",
					 "Date", "Elderberry", "Fig",
					 "Grape", "Honeydew", "Kiwi",
					 "Lemon", "Mango", "Nectarine",
					 "Orange", "Papaya", "Quince",
					 "Ilama", "Jabuticaba", "Kiwano",
					 "Longan", "Mandarin", "Naranjilla",
					  "Grape", "Honeydew", "Kiwi",
					 "Lemon", "Mango", "Nectarine",
					 "Orange", "Papaya", "Quince",
					 "Ilama", "Jabuticaba", "Kiwano",
					 "Longan", "Mandarin", "Naranjilla",
					 "Olallieberry", "Persimmon",
					 "Rose Apple", "Soursop",
					 "Grape", "Honeydew", "Kiwi",
					 "Lemon", "Mango", "Nectarine",
					 "Orange", "Papaya", "Quince",
					 };
				foreach (var fruit in fruitsArray)
				{
					hStack.Children.Add(new Label { Text = fruit, FontSize = 16, Margin = new Thickness(10, 0) });
				}
				vm.Content = hStack;
				break;
		}
	}
	private void IsVisibleRadio_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!(sender is RadioButton rb) || !rb.IsChecked)
			return;
		_viewModel.IsVisible = rb.Content?.ToString() == "True";
	}

	private void OnFlowDirectionChanged(object sender, EventArgs e)
	{
		_viewModel.FlowDirection = FlowDirectionLTR.IsChecked ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
	}

	private void OrientationRadio_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!(sender is RadioButton rb) || !rb.IsChecked)
			return;

		switch (rb.Content?.ToString())
		{
			case "Vertical":
				_viewModel.Orientation = ScrollOrientation.Vertical;
				break;
			case "Horizontal":
				_viewModel.Orientation = ScrollOrientation.Horizontal;
				break;
			case "Both":
				_viewModel.Orientation = ScrollOrientation.Both;
				break;
			case "Neither":
				_viewModel.Orientation = ScrollOrientation.Neither;
				break;
		}
	}
	private void HorizontalSBRadio_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!(sender is RadioButton rb) || !rb.IsChecked)
			return;
		switch (rb.Content?.ToString())
		{
			case "Always":
				_viewModel.HorizontalScrollBarVisibility = ScrollBarVisibility.Always;
				break;
			case "Never":
				_viewModel.HorizontalScrollBarVisibility = ScrollBarVisibility.Never;
				break;
			default:
				_viewModel.HorizontalScrollBarVisibility = ScrollBarVisibility.Default;
				break;
		}
	}

	private void IsEnabledRadio_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!(sender is RadioButton rb) || !rb.IsChecked)
			return;
		_viewModel.IsEnabled = rb.Content?.ToString() == "True";
	}

	private void VerticalSBRadio_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!(sender is RadioButton rb) || !rb.IsChecked)
			return;
		switch (rb.Content?.ToString())
		{
			case "Always":
				_viewModel.VerticalScrollBarVisibility = ScrollBarVisibility.Always;
				break;
			case "Never":
				_viewModel.VerticalScrollBarVisibility = ScrollBarVisibility.Never;
				break;
			default:
				_viewModel.VerticalScrollBarVisibility = ScrollBarVisibility.Default;
				break;
		}
	}
}