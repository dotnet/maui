using System;
using Xamarin.Forms.Controls.Issues;

namespace Xamarin.Forms.Controls
{
	public class MasterDetailPageTabletPage : ContentPage
	{
		public MasterDetailPageTabletPage ()
		{
			Title = "MasterDetail MasterBehavior Gallery";

			var btn = new Button { Text = "Default (old behavior)" };
			btn.Clicked += async (sender, e) => await Navigation.PushModalAsync (new Issue1461Page (MasterBehavior.Default,null));

			var btn1 = new Button { Text = "Default (old behavior) but always hide toolbar button" };
			btn1.Clicked += async (sender, e) => await Navigation.PushModalAsync (new Issue1461Page (MasterBehavior.Default,false));

			var btn2 = new Button { Text = "Popover - (always show it as a popover) but  hide toolbar button" };
			btn2.Clicked += async (sender, e) => await Navigation.PushModalAsync (new Issue1461Page (MasterBehavior.Popover,false));

			//there's never a time when to real hide the master i think, use a normal page for that.. 
			//what the user wants is out of the screen, maybe we need a better namming
			var btn3 = new Button { Text = "Popover - (always show it as a popover)" };
			btn3.Clicked += async (sender, e) => await Navigation.PushModalAsync (new Issue1461Page (MasterBehavior.Popover, null));

			//we throw an exception here if you try to toggle it 
			var btn4 = new Button { Text = "Split - (always show it as splitview , toggle master always visible, throws exception)" };
			btn4.Clicked += async (sender, e) => await Navigation.PushModalAsync (new Issue1461Page (MasterBehavior.Split, null));

			var btn5 = new Button { Text = "SplitOnPortrait Portrait - (always show it as splitview in Portrait, throws exception)" };
			btn5.Clicked += async (sender, e) => await Navigation.PushModalAsync (new Issue1461Page (MasterBehavior.SplitOnPortrait, null));

			var btn6 = new Button { Text = "SplitOnLandscape Landscape - (always show it as splitview in Landscape, throws exception))" };
			btn6.Clicked += async (sender, e) => await Navigation.PushModalAsync (new Issue1461Page (MasterBehavior.SplitOnLandscape, null));

			Content = new StackLayout { Padding= new Thickness(0,20,0,0) , Children = { btn, btn1, btn2, btn6, btn3, btn4, btn5 }, Spacing = 20 };

		}
	}
}

