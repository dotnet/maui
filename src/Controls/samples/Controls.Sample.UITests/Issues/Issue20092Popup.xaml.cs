using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Issue20092Popup : CommunityToolkit.Maui.Views.Popup
	{
		public Issue20092Popup()
		{
			InitializeComponent();
		}
	}
}