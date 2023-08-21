//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2563, "NavigationPage should support queuing of navigation events", PlatformAffected.Android | PlatformAffected.WinPhone | PlatformAffected.iOS)]
	public class Issue2563 : ContentPage
	{
		public Issue2563()
		{
			var button = new Button
			{
				Text = "Click Me",
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center
			};

			Content = button;

			var random = new Random();
			button.Clicked += (sender, args) =>
			{
				for (int i = 0; i < 10; i++)
				{
					button.Navigation.PushAsync(new ContentPage
					{
						Title = "Page " + i,
						Content = new Label
						{
							Text = "Page " + i,
							HorizontalTextAlignment = TextAlignment.Center,
							VerticalTextAlignment = TextAlignment.Center
						}
					}, random.NextDouble() > 0.5);
				}

				for (int i = 0; i < 6; i++)
				{
					button.Navigation.PopAsync(random.NextDouble() > 0.5);
				}

				button.Navigation.PopToRootAsync(random.NextDouble() > 0.5);
			};
		}
	}
}
