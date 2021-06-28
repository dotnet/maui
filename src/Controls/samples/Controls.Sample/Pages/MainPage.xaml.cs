using System;
using Maui.Controls.Sample.ViewModels;

namespace Maui.Controls.Sample.Pages
{
	public partial class MainPage
	{
		public MainPage(IServiceProvider services, MainViewModel viewModel)
		{
			InitializeComponent();

			BindingContext = viewModel;
		}
	}
}