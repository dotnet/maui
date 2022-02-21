using System.Diagnostics;

namespace Maui.Controls.Sample.Pages
{
	public partial class InputTransparentPage
	{
		public InputTransparentPage()
		{
			InitializeComponent();
		}

		void OnButtonClicked(object sender, System.EventArgs e)
		{
			Debug.WriteLine("Clicked");
		}
	}
}