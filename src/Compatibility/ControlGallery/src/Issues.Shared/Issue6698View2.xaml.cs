#if !UITEST
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Issue6698View2
	{
		public Issue6698View2()
		{
			InitializeComponent();
		}
	}
}
#endif