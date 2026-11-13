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

		var refreshView = new RefreshView
		{
			AutomationId = "RefreshView",
			Content = new ScrollView
			{
				Content = boxView
			}
		};

		refreshView.Refreshing += OnRefreshViewRefreshing;

		refreshView.SetBinding(RefreshView.CommandProperty, "Command");
		refreshView.SetBinding(RefreshView.CommandParameterProperty, "CommandParameter");
		refreshView.SetBinding(RefreshView.FlowDirectionProperty, "FlowDirection");
		refreshView.SetBinding(RefreshView.IsEnabledProperty, "IsEnabled");
		refreshView.SetBinding(RefreshView.IsVisibleProperty, "IsVisible");
		refreshView.SetBinding(RefreshView.IsRefreshingProperty, "IsRefreshing");
		refreshView.SetBinding(RefreshView.RefreshColorProperty, "RefreshColor");
		refreshView.SetBinding(RefreshView.ShadowProperty, "Shadow");

		RefreshViewContainer.Children.Add(refreshView);
	}

	private void SetCollectionViewContent()
	{
		RefreshViewContainer.Children.Clear();
		var refreshView = new RefreshView
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

		refreshView.Refreshing += OnRefreshViewRefreshing;


		refreshView.SetBinding(RefreshView.CommandProperty, "Command");
		refreshView.SetBinding(RefreshView.CommandParameterProperty, "CommandParameter");
		refreshView.SetBinding(RefreshView.FlowDirectionProperty, "FlowDirection");
		refreshView.SetBinding(RefreshView.IsEnabledProperty, "IsEnabled");
		refreshView.SetBinding(RefreshView.IsVisibleProperty, "IsVisible");
		refreshView.SetBinding(RefreshView.IsRefreshingProperty, "IsRefreshing");
		refreshView.SetBinding(RefreshView.RefreshColorProperty, "RefreshColor");
		refreshView.SetBinding(RefreshView.ShadowProperty, "Shadow");

		RefreshViewContainer.Children.Add(refreshView);
	}

	private void OnScrollViewContentClicked(object sender, EventArgs e)
	{
		SetScrollViewContent();
	}

	private void OnCollectionViewContentClicked(object sender, EventArgs e)
	{
		SetCollectionViewContent();
	}

	private void OnRefreshViewRefreshing(object sender, EventArgs e)
{
    if (BindingContext is RefreshViewViewModel vm)
    {
        vm.RefreshEventStatusText = "Raised";
    }
}
}
