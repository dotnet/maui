using System.Diagnostics;
using Maui.Controls.Sample.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class XamlPage : BasePage
	{
		public XamlPage()
		{
			InitializeComponent();

			foreach (var x in MyLayout)
			{
				Debug.WriteLine($"{x}");
			}
		}
	}
}