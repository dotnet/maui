namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30004, "FontImageSource not center-aligned inside Image control", PlatformAffected.UWP)]
public class Issue30004 : ContentPage
{
	public Issue30004()
	{
		Content = new StackLayout()
		{
			Children =
				{
					new Label(){Text = "FontAwesome", AutomationId="FontImage", HorizontalOptions = LayoutOptions.Center, FontSize = 20, Margin = new Thickness(0, 20, 0, 20)},
					new Image() { Source = new FontImageSource() {  FontFamily = "FA", Glyph = "\xf7a4", Color = Colors.Black, Size = 50}, Margin = 4, Background= Colors.Red, WidthRequest=100, HeightRequest=100, HorizontalOptions = LayoutOptions.Center},

					new Label(){Text = "ionicons", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, FontSize = 20, Margin = new Thickness(0, 20, 0, 20)},
					new Image() { Source = new FontImageSource() { FontFamily = "Ion",Glyph = "\uf47e", Color = Colors.Black, Size = 50},  Background= Colors.Red,WidthRequest=100, HeightRequest=100, HorizontalOptions = LayoutOptions.Center},
				}
		};
	}
}