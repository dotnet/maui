using System;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class ListViewContextActions
	{
		public ListViewContextActions()
		{
			InitializeComponent();
			listView.ItemsSource =
				Enumerable
					.Range(0, 100)
					.Select(i => new ContextListViewItem(i, $"ListView item {i}"))
					.ToList();
		}

		private void OnDetailClicked(object sender, EventArgs e)
		{
			var mi = ((MenuItem)sender);
			DisplayAlertAsync("Detail Action", $"Details for item {mi.CommandParameter}", "OK");
		}

		private void OnDeleteClicked(object sender, EventArgs e)
		{
			var mi = ((MenuItem)sender);
			DisplayAlertAsync("Delete Action", $"Deleting item {mi.CommandParameter}!", "OK");
		}

		private record ContextListViewItem(int Index, string Text);
	}
}
