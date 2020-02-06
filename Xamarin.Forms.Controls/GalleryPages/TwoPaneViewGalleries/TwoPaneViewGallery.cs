using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms.Controls.GalleryPages.TwoPaneViewGalleries
{
	public class TwoPaneViewGallery : ContentPage
	{
		public TwoPaneViewGallery()
		{
			if(Application.Current.MainPage is MasterDetailPage mdp)
			{
				mdp.MasterBehavior = MasterBehavior.Popover;
				mdp.IsPresented = false;
			}

			Content = new StackLayout
			{
				Children =
				{
					GalleryBuilder.NavButton("Nested TwoPaneView Split Across Hinge", () => new NestedTwoPaneViewSplitAcrossHinge(), Navigation),
				}
			};
		}
	}
}
