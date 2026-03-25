using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class ContentViewControlPage : NavigationPage
{
	public ContentViewControlPage()
	{
		var vm = new ContentViewViewModel();
		var mainPage = new ContentViewControlMainPage(vm);
		PushAsync(mainPage);
	}
}
public class ContentViewControlMainPage : ContentPage
{
	private ContentView _dynamicContentHost;
	private ContentViewViewModel _viewModel;


	public ContentViewControlMainPage(ContentViewViewModel viewModel)
	{
		_viewModel = viewModel;
		BindingContext = viewModel;

		this.SetBinding(ContentPage.BackgroundColorProperty, nameof(ContentViewViewModel.BackgroundColor));
		this.SetBinding(ContentPage.IsEnabledProperty, nameof(ContentViewViewModel.IsEnabled));
		this.SetBinding(ContentPage.IsVisibleProperty, nameof(ContentViewViewModel.IsVisible));
		this.SetBinding(ContentPage.FlowDirectionProperty, nameof(ContentViewViewModel.FlowDirection));
		_dynamicContentHost = new ContentView();

		UpdateContentViews();

		var mainLayout = new VerticalStackLayout
		{
			Spacing = 2,
			Padding = new Thickness(5),
			Children =
		{
			_dynamicContentHost,
		}
		};

		Content = mainLayout;

		ToolbarItems.Add(new ToolbarItem
		{
			Text = "Options",
			Command = new Command(async () =>
			{
				_viewModel.HeightRequest = -1;
				_viewModel.WidthRequest = -1;
				_viewModel.BackgroundColor = Colors.LightGray;
				_viewModel.IsEnabled = true;
				_viewModel.IsVisible = true;
				_viewModel.FlowDirection = FlowDirection.LeftToRight;
				_viewModel.HasShadow = false;
				_viewModel.DefaultLabelText = "This is Default Page";
				_viewModel.ContentLabel = "Default";
				_viewModel.ControlTemplateKeyFirst = "DefaultFirstTemplate";
				_viewModel.ControlTemplateKeySecond = "DefaultSecondTemplate";
				_viewModel.IconImageSource = "dotnet_bot.png";
				_viewModel.CardTitle = "First ContentView Page";
				_viewModel.CardColor = Colors.SkyBlue;

				UpdateContentViews();
				await Navigation.PushAsync(new ContentViewOptionsPage(_viewModel));
			})
		});

		viewModel.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == nameof(ContentViewViewModel.ContentLabel)
				|| e.PropertyName == nameof(ContentViewViewModel.ControlTemplateKeyFirst)
				|| e.PropertyName == nameof(ContentViewViewModel.ControlTemplateKeySecond))
			{
				UpdateContentViews();
			}
		};
	}
	private void UpdateContentViews()
	{
		if (_viewModel.ContentLabel == "FirstCustomPage")
		{
			var firstView = new ContentViewFirstCustomPage
			{
				CardDescription = "Use ContentViewPage as the content, binding all card properties to the ViewModel",
				IconBackgroundColor = Colors.LightGray,
				BorderColor = Colors.Pink
			};
			firstView.SetBinding(ContentViewFirstCustomPage.CardTitleProperty, nameof(ContentViewViewModel.CardTitle));
			firstView.SetBinding(ContentViewFirstCustomPage.IconImageSourceProperty, nameof(ContentViewViewModel.IconImageSource));
			firstView.SetBinding(ContentViewFirstCustomPage.CardColorProperty, nameof(ContentViewViewModel.CardColor));
			firstView.BindingContext = _viewModel;
			if (!string.IsNullOrEmpty(_viewModel.ControlTemplateKeyFirst) && firstView.Resources.ContainsKey(_viewModel.ControlTemplateKeyFirst))
				firstView.ControlTemplate = (ControlTemplate)firstView.Resources[_viewModel.ControlTemplateKeyFirst];
			_dynamicContentHost.Content = firstView;
		}
		else if (_viewModel.ContentLabel == "SecondCustomPage")
		{
			var secondView = new ContentViewSecondCustomPage
			{
				SecondCustomViewTitle = "Second Custom Title",
				SecondCustomViewDescription = "This is the description for the second custom view.",
				SecondCustomViewText = "This is the SECOND custom view",
				FrameBackgroundColor = Colors.LightGray
			};
			secondView.BindingContext = _viewModel;
			if (!string.IsNullOrEmpty(_viewModel.ControlTemplateKeySecond) && secondView.Resources.ContainsKey(_viewModel.ControlTemplateKeySecond))
				secondView.ControlTemplate = (ControlTemplate)secondView.Resources[_viewModel.ControlTemplateKeySecond];
			_dynamicContentHost.Content = secondView;
		}
		else
		{
			var defaultLabel = new Label
			{
				FontSize = 18
			};
			defaultLabel.SetBinding(Label.TextProperty, nameof(ContentViewViewModel.DefaultLabelText));
			var button = new Button
			{
				Text = "Change Text",
				AutomationId = "TextChangedButton"
			};
			button.Clicked += (s, e) =>
			{
				_viewModel.DefaultLabelText = "Text Changed after Button Click!";
			};
			var stack = new StackLayout
			{
				Spacing = 12,
				Padding = new Thickness(20),
				BackgroundColor = Colors.LightYellow,
				Children = { defaultLabel, button }
			};
			stack.BindingContext = _viewModel;
			_dynamicContentHost.Content = stack;
			_dynamicContentHost.SetBinding(ContentView.HeightRequestProperty, nameof(ContentViewViewModel.HeightRequest));
			_dynamicContentHost.SetBinding(ContentView.WidthRequestProperty, nameof(ContentViewViewModel.WidthRequest));
			_dynamicContentHost.SetBinding(ContentView.ShadowProperty, nameof(ContentViewViewModel.ContentViewShadow));
		}
	}
}
