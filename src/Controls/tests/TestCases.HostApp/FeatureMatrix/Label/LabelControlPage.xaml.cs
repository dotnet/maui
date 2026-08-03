using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class LabelControlPage : NavigationPage
{
	private LabelViewModel _viewModel;

	public LabelControlPage()
	{
		_viewModel = new LabelViewModel();
		PushAsync(new LabelControlMainPage(_viewModel));
	}
}

public partial class LabelControlMainPage : ContentPage
{
	private LabelViewModel _viewModel;

#if WINDOWS
	private readonly VerticalStackLayout _mainLabelHost;
	private Label _mainLabel;
#endif

	public LabelControlMainPage(LabelViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;

#if WINDOWS
		_mainLabelHost = (VerticalStackLayout)Content;
		_mainLabel = (Label)_mainLabelHost.Children[0];
#endif
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new LabelViewModel();
		await Navigation.PushAsync(new LabelOptionsPage(_viewModel));
	}

	void MainLabel_Tapped(object sender, TappedEventArgs e)
	{
#if WINDOWS
		var oldLabel = _mainLabel;

		oldLabel.BindingContext = null;
		oldLabel.RemoveBinding(Label.FormattedTextProperty);
		oldLabel.FormattedText = null;
		_mainLabelHost.Children.Remove(oldLabel);

		_mainLabel = CreateMainLabel();
		_mainLabelHost.Children.Add(_mainLabel);
#else
		// Recreate the page to verify initial mappers
		// Clear BindingContext first so old Label properly detaches the FormattedString
		// (triggers propertyChanging which unsubscribes events and calls RemoveSpans)
		ToolbarItems.Clear();
		BindingContext = null;
		Content = new ContentView();
		InitializeComponent();
		BindingContext = _viewModel;
#endif
	}

#if WINDOWS
	Label CreateMainLabel()
	{
		// Use the same XAML as the initial control so all local bindings and values stay identical.
		var templatePage = new LabelControlMainPage(_viewModel);
		var templateHost = (VerticalStackLayout)templatePage.Content;
		var label = (Label)templateHost.Children[0];

		templateHost.Children.Remove(label);
		label.GestureRecognizers.Clear();

		var tapGestureRecognizer = new TapGestureRecognizer();
		tapGestureRecognizer.Tapped += MainLabel_Tapped;
		label.GestureRecognizers.Add(tapGestureRecognizer);

		return label;
	}
#endif
}