using Maui.Controls.Sample.Pages.Base;

namespace Maui.Controls.Sample.Pages
{
	public partial class EilonPage : BasePage
	{
		public EilonPage()
		{
			InitializeComponent();

			ItemsList.ItemsSource = new string[] { "Hello", "World" };
		}

		private void MenuItem_Clicked(object sender, System.EventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("Detail clicked");
		}

		private void MenuItem_Clicked_1(object sender, System.EventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("Delete clicked");
		}
	}
}
