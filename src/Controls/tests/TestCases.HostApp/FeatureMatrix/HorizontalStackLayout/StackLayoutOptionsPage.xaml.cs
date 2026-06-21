using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class StackLayoutOptionsPage : ContentPage
{
	private StackLayoutViewModel _viewModel;

	public StackLayoutOptionsPage(StackLayoutViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private void ApplyButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}
}