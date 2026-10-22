using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class RefreshViewPage
	{
		public RefreshViewPage()
		{
			InitializeComponent();
		}

		private void OnToggleRefreshColorClicked(object? sender, System.EventArgs e)
		{
			if (refreshView.RefreshColor == Colors.Red)
			{
				refreshView.RefreshColor = Colors.Teal;
			}
			else
			{
				refreshView.RefreshColor = Colors.Red;
			}
		}

		private void OnToggleRefreshBackgroundColorClicked(object? sender, System.EventArgs e)
		{
			if (refreshView.Background == SolidColorBrush.Yellow)
			{
				refreshView.Background = SolidColorBrush.Green;
			}
			else
			{
				refreshView.Background = SolidColorBrush.Yellow;
			}
		}

		private void OnTriggerRefreshClicked(object? sender, System.EventArgs e) =>
			refreshView.IsRefreshing = !refreshView.IsRefreshing;

		private void OnToggleEnabledClicked(object? sender, System.EventArgs e) =>
			refreshView.IsEnabled = !refreshView.IsEnabled;
	}
}
