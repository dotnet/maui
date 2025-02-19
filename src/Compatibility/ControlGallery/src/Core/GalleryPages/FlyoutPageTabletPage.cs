using System;
using Microsoft.Maui.Controls.ControlGallery.Issues;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public class FlyoutPageTabletPage : ContentPage
	{
		public FlyoutPageTabletPage()
		{
			Title = "FlyoutPage FlyoutLayoutBehavior Gallery";

			var btn = new Button { Text = "Default (old behavior)" };
			btn.Clicked += async (sender, e) => await Navigation.PushModalAsync(new Issue1461Page(FlyoutLayoutBehavior.Default, null));

			var btn1 = new Button { Text = "Default (old behavior) but always hide toolbar button" };
			btn1.Clicked += async (sender, e) => await Navigation.PushModalAsync(new Issue1461Page(FlyoutLayoutBehavior.Default, false));

			var btn2 = new Button { Text = "Popover - (always show it as a popover) but  hide toolbar button" };
			btn2.Clicked += async (sender, e) => await Navigation.PushModalAsync(new Issue1461Page(FlyoutLayoutBehavior.Popover, false));

			//there's never a time when to real hide the master i think, use a normal page for that.. 
			//what the user wants is out of the screen, maybe we need a better namming
			var btn3 = new Button { Text = "Popover - (always show it as a popover)" };
			btn3.Clicked += async (sender, e) => await Navigation.PushModalAsync(new Issue1461Page(FlyoutLayoutBehavior.Popover, null));

			//we throw an exception here if you try to toggle it 
			var btn4 = new Button { Text = "Split - (always show it as splitview , toggle master always visible, throws exception)" };
			btn4.Clicked += async (sender, e) => await Navigation.PushModalAsync(new Issue1461Page(FlyoutLayoutBehavior.Split, null));

			var btn5 = new Button { Text = "SplitOnPortrait Portrait - (always show it as splitview in Portrait, throws exception)" };
			btn5.Clicked += async (sender, e) => await Navigation.PushModalAsync(new Issue1461Page(FlyoutLayoutBehavior.SplitOnPortrait, null));

			var btn6 = new Button { Text = "SplitOnLandscape Landscape - (always show it as splitview in Landscape, throws exception))" };
			btn6.Clicked += async (sender, e) => await Navigation.PushModalAsync(new Issue1461Page(FlyoutLayoutBehavior.SplitOnLandscape, null));

			Content = new StackLayout { Padding = new Thickness(0, 20, 0, 0), Children = { btn, btn1, btn2, btn6, btn3, btn4, btn5 }, Spacing = 20 };

		}
	}
}

