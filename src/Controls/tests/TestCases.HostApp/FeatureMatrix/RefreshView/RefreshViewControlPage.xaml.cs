namespace Maui.Controls.Sample;

public class RefreshViewControlPage : NavigationPage
{
	private RefreshViewViewModel _viewModel;

	public RefreshViewControlPage()
	{
		_viewModel = new RefreshViewViewModel();
#if ANDROID
			BarTextColor = Colors.White;
#endif
		PushAsync(new RefreshViewControlMainPage(_viewModel));
	}
}

public partial class RefreshViewControlMainPage : ContentPage
{
	private RefreshViewViewModel _viewModel;

	public RefreshViewControlMainPage(RefreshViewViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;

		SetScrollViewContent();
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		SetScrollViewContent();
		BindingContext = _viewModel = new RefreshViewViewModel();
		await Navigation.PushAsync(new RefreshViewOptionsPage(_viewModel));
	}

	private void SetScrollViewContent()
	{
		RefreshViewContainer.Children.Clear();

		// Create BoxView separately
		var boxView = new BoxView
		{
			HeightRequest = 100,
			WidthRequest = 200,
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "BoxContent"
		};

		// Set binding immediately
		boxView.SetBinding(BoxView.ColorProperty, "BoxViewColor");

		var RefreshView = new RefreshView
		{
			AutomationId = "RefreshView",
			Content = new ScrollView
			{
				Content = boxView
			}
		};

		RefreshView.SetBinding(RefreshView.CommandProperty, "Command");
		RefreshView.SetBinding(RefreshView.CommandParameterProperty, "CommandParameter");
		RefreshView.SetBinding(RefreshView.FlowDirectionProperty, "FlowDirection");
		RefreshView.SetBinding(RefreshView.IsEnabledProperty, "IsEnabled");
		RefreshView.SetBinding(RefreshView.IsVisibleProperty, "IsVisible");
		RefreshView.SetBinding(RefreshView.IsRefreshingProperty, "IsRefreshing");
		RefreshView.SetBinding(RefreshView.RefreshColorProperty, "RefreshColor");
		RefreshView.SetBinding(RefreshView.ShadowProperty, "Shadow");

		RefreshViewContainer.Children.Add(RefreshView);
	}

	private void SetCollectionViewContent()
	{
		RefreshViewContainer.Children.Clear();
		var RefreshView = new RefreshView
		{
			AutomationId = "RefreshView",
			Content = new CollectionView
			{
				ItemsSource = new List<string>
				{
					"Item 1", "Item 2", "Item 3", "Item 4", "Item 5",
					"Item 6", "Item 7", "Item 8", "Item 9", "Item 10"
				},
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label
					{
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						FontSize = 16,
						Padding = new Thickness(10)
					};
					label.SetBinding(Label.TextProperty, ".");
					return label;
				})
			}
		};

		RefreshView.SetBinding(RefreshView.CommandProperty, "Command");
		RefreshView.SetBinding(RefreshView.CommandParameterProperty, "CommandParameter");
		RefreshView.SetBinding(RefreshView.FlowDirectionProperty, "FlowDirection");
		RefreshView.SetBinding(RefreshView.IsEnabledProperty, "IsEnabled");
		RefreshView.SetBinding(RefreshView.IsVisibleProperty, "IsVisible");
		RefreshView.SetBinding(RefreshView.IsRefreshingProperty, "IsRefreshing");
		RefreshView.SetBinding(RefreshView.RefreshColorProperty, "RefreshColor");
		RefreshView.SetBinding(RefreshView.ShadowProperty, "Shadow");

		RefreshViewContainer.Children.Add(RefreshView);
	}

	private void OnScrollViewContentClicked(object sender, EventArgs e)
	{
		SetScrollViewContent();
	}

	private void OnCollectionViewContentClicked(object sender, EventArgs e)
	{
		SetCollectionViewContent();
	}
}