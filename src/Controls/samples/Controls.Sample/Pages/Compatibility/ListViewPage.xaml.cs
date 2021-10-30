using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class ListViewPage
	{
		public ListViewPage()
		{
			InitializeComponent();
		}

		void OnItemTapped(object sender, ItemTappedEventArgs e)
		{
			if (e == null)
				return; // Has been set to null, do not 'process' tapped event

			Debug.WriteLine("Tapped: " + e.Item);
			((ListView)sender).SelectedItem = null; // De-select the row
		}
	}
}