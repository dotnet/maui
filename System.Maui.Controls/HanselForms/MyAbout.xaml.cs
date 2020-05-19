using System;
using System.Collections.Generic;

using System.Maui;

namespace System.Maui.Controls
{
	public partial class MyAbout : ContentPage
	{
		public MyAbout()
		{
			InitializeComponent();

			twitter.GestureRecognizers.Add(new TapGestureRecognizer()
			{
				Command = new Command(async () =>
				{

					await this.Navigation.PushAsync(new WebsiteView("https://m.twitter.com/shanselman", "@shanselman"));
				})
			});

			facebook.GestureRecognizers.Add(new TapGestureRecognizer()
			{
				Command = new Command(async () =>
				{

					await this.Navigation.PushAsync(new WebsiteView("https://facebook.com/scott.hanselman", "Scott @Facebook"));
				})
			});

			instagram.GestureRecognizers.Add(new TapGestureRecognizer()
			{
				Command = new Command(async () =>
				{

					await this.Navigation.PushAsync(new WebsiteView("https://instagram.com/shanselman", "Scott @Instagram"));
				})
			});
		}
	}
}
