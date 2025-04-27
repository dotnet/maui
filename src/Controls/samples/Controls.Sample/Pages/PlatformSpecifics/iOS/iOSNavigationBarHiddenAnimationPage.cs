using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Pages;

public class iOSNavigationBarHiddenAnimationPage : ContentPage
{
	public iOSNavigationBarHiddenAnimationPage()
	{
		var setHasNavigationBarSwitch = new Microsoft.Maui.Controls.Switch() { IsToggled = true };
		setHasNavigationBarSwitch.Toggled += (s, e) => Microsoft.Maui.Controls.NavigationPage.SetHasNavigationBar(this, e.Value);
		var setNavigationBarHiddenAnimation = new Microsoft.Maui.Controls.Switch() { IsToggled = true };
		setNavigationBarHiddenAnimation.Toggled += (s, e) => On<iOS>().SetNavigationBarHiddenAnimation(e.Value);

		Content = new VerticalStackLayout
		{
			Spacing = 5,
			Children =
				{
					new HorizontalStackLayout
					{
						Children =
						{
							new Label {
								Text = "NavigationPage.SetHasNavigationBar: ",
							},
							setHasNavigationBarSwitch,
						}
					},
					new HorizontalStackLayout
					{
						Children =
						{
							new Label {
								Text = "Page.NavigationBarHiddenAnimation: ",
							},
							setNavigationBarHiddenAnimation,
						}
					}
				}
		};
	}
}
