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
	private Label _mainLabel;

	public LabelControlMainPage(LabelViewModel viewModel)
	{
		InitializeComponent();
		_mainLabel = (Label)MainLabelHost.Children[0];
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new LabelViewModel();
		await Navigation.PushAsync(new LabelOptionsPage(_viewModel));
	}

	void MainLabel_Tapped(object sender, TappedEventArgs e)
	{
		var oldLabel = _mainLabel;
		var style = oldLabel.Style ?? throw new InvalidOperationException("MainLabel style is required.");

		oldLabel.BindingContext = null;
		oldLabel.RemoveBinding(Label.FormattedTextProperty);
		oldLabel.FormattedText = null;
		MainLabelHost.Children.Remove(oldLabel);

		_mainLabel = CreateMainLabel(style);
		MainLabelHost.Children.Add(_mainLabel);
	}

	Label CreateMainLabel(Style style)
	{
		var label = new Label
		{
			Style = style,
			BindingContext = _viewModel,
		};

		var tapGestureRecognizer = new TapGestureRecognizer();
		tapGestureRecognizer.Tapped += MainLabel_Tapped;
		label.GestureRecognizers.Add(tapGestureRecognizer);

		return label;
	}
}