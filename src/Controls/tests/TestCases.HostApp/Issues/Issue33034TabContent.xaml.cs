using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class Issue33034TabContent : ContentPage
{
	public Issue33034TabContent()
	{
		InitializeComponent();
	}
}
