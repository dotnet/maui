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

	public LabelControlMainPage(LabelViewModel viewModel)
	{
		InitializeComponent();
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
#if WINDOWS
		// Recreate only the handler while preserving the page, toolbar, and layout.
		// Re-parenting restores the inherited bindings before the new handler connects.
		var index = MainLabelHost.Children.IndexOf(MainLabel);
		if (index < 0)
			throw new InvalidOperationException("MainLabel is not attached to its host.");

		MainLabelHost.Children.RemoveAt(index);
		MainLabel.Handler = null;
		MainLabelHost.Children.Insert(index, MainLabel);
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
}