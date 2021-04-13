using System.Linq;
using Maui.Controls.Sample.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class XamlPage : BasePage
	{
		public XamlPage()
		{
			InitializeComponent();
			var window = Microsoft.Maui.Controls.Application.Current.Windows.FirstOrDefault();
		}

		protected override void OnAppearing()
		{
		
			base.OnAppearing();
		}
	}
}