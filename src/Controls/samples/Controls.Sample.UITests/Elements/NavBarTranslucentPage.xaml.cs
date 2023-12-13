using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class NavBarTranslucentPage : ContentView
{
	public NavBarTranslucentPage()
	{
		InitializeComponent();
		if (Microsoft.Maui.Controls.Application.Current.MainPage is Microsoft.Maui.Controls.NavigationPage nav)
		{
			nav.BarBackgroundColor = Colors.Transparent;
			nav.On<iOS>().SetHideNavigationBarSeparator(true);
			nav.On<iOS>().EnableTranslucentNavigationBar();
		}
	}
}
