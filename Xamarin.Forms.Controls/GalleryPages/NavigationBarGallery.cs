using System.Diagnostics;

namespace Xamarin.Forms.Controls
{
	public class NavigationBarGallery : ContentPage
	{
		public NavigationBarGallery (NavigationPage rootNavPage)
		{

			int toggleBarTextColor = 0;
			int toggleBarBackgroundColor = 0;

			Content = new StackLayout {
				Children = {
					new Button {
						Text = "Change BarTextColor", 
						Command = new Command (() => {
							if (toggleBarTextColor % 2 == 0) {
								rootNavPage.BarTextColor = Color.Teal;	
							} else {
								rootNavPage.BarTextColor = Color.Default;
							}
							toggleBarTextColor++;
						})
					},
					new Button {
						Text = "Change BarBackgroundColor", 
						Command = new Command (() => {
							if (toggleBarBackgroundColor % 2 == 0) {
								rootNavPage.BarBackgroundColor = Color.Navy;
							} else {
								rootNavPage.BarBackgroundColor = Color.Default;
							}
							toggleBarBackgroundColor++;

						})
					},
					new Button {
						Text = "Change Both to default", 
						Command = new Command (() => {
							rootNavPage.BarTextColor = Color.Default;
							rootNavPage.BarBackgroundColor = Color.Default;
						})
					},
					new Button {
						Text = "Make sure Tint still works", 
						Command = new Command (() => {
#pragma warning disable 618
							rootNavPage.Tint = Color.Red;
#pragma warning restore 618
						})
					},
					new Button {
						Text = "Black background, white text", 
						Command = new Command (() => {
							rootNavPage.BarTextColor = Color.White;
							rootNavPage.BarBackgroundColor = Color.Black;
						})
					}
				}
			};
		}
	}
}