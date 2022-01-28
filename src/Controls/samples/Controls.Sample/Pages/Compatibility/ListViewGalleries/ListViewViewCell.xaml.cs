using System.Linq;

namespace Maui.Controls.Sample.Pages
{
	public partial class ListViewViewCell
	{
		public ListViewViewCell()
		{
			InitializeComponent();
			listView.ItemsSource = Enumerable.Range(0, 100).Select(i => $" Text {i}").ToList();
		}
	}
}