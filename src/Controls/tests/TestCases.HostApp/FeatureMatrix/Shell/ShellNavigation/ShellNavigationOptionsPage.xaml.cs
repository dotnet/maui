using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public partial class ShellNavigationOptionsPage : ContentPage
	{
		readonly ShellViewModel _viewModel;

		public ShellNavigationOptionsPage(ShellViewModel viewModel)
		{
			_viewModel = viewModel;
			BindingContext = _viewModel;
			InitializeComponent();

			this.Appearing += OnPageAppearing;
		}

		void OnPageAppearing(object sender, System.EventArgs e)
		{
			var shell = Shell.Current;
			if (shell != null)
			{
				_viewModel.CurrentState = (shell.CurrentState?.Location?.ToString() ?? "Not Set").TrimStart('/');
				_viewModel.CurrentPage = shell.CurrentPage?.Title ?? "Not Set";
				_viewModel.CurrentItem = shell.CurrentItem?.Title ?? "Not Set";
				_viewModel.ShellCurrent = shell.GetType().Name;
			}
		}
	}
}
