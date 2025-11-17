using Maui.Controls.Sample.ViewModels;

namespace Maui.Controls.Sample.Pages;

public partial class MainPage : ContentPage
{
	public MainPage(ChatViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
	}
}
