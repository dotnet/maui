using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 17022, "UINavigationBar is Translucent", PlatformAffected.iOS)]
public partial class Issue17022 : ContentPage
{
	Color _initialBarBackgroundColor;
	bool _initialTranslucent;

	public Issue17022()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		if (Parent is Microsoft.Maui.Controls.NavigationPage nav)
		{
			_initialBarBackgroundColor = nav.BarBackgroundColor;
			_initialTranslucent = nav.On<iOS>().IsNavigationBarTranslucent();

			nav.BarBackgroundColor = Colors.Transparent;
			nav.On<iOS>().SetHideNavigationBarSeparator(true);
			nav.On<iOS>().EnableTranslucentNavigationBar();
		}
	}
	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		if (Parent is Microsoft.Maui.Controls.NavigationPage nav)
		{
			nav.BarBackgroundColor = _initialBarBackgroundColor;
			if (_initialTranslucent)
				nav.On<iOS>().EnableTranslucentNavigationBar();
			else
				nav.On<iOS>().DisableTranslucentNavigationBar();
		}
	}
}
