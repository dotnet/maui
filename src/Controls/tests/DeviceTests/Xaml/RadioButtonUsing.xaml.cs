using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.DeviceTests
{
#pragma warning disable CS0618 // XamlCompilationAttribute is deprecated, remove this in .NET 12
	[XamlCompilation(XamlCompilationOptions.Compile)]
#pragma warning restore CS0618
	public partial class RadioButtonUsing : ContentPage
	{
		public RadioButtonUsing()
		{
			InitializeComponent();
		}
	}
}
