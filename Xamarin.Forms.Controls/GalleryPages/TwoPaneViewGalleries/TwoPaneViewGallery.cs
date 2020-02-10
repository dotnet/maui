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

			var twoPaneView = new DualScreen.TwoPaneView()
			{
				MinTallModeHeight  = 0,
				MinWideModeWidth = 0
			};

			var pane1 = new StackLayout
			{
				Children =
				{

					GalleryBuilder.NavButton("TwoPanePropertiesGallery", () => new TwoPanePropertiesGallery(), Navigation),
					GalleryBuilder.NavButton("Master Details Sample", () => new MasterDetail(), Navigation),
					GalleryBuilder.NavButton("Companion Pane", () => new CompanionPane(), Navigation),
					GalleryBuilder.NavButton("ExtendCanvas Sample", () => new ExtendCanvas(), Navigation),
					GalleryBuilder.NavButton("DualViewMapPage Sample", () => new DualViewMapPage(), Navigation),
				}
			};

			var pane2 = new StackLayout
			{
				Children =
				{
					GalleryBuilder.NavButton("Nested TwoPaneView Split Across Hinge", () => new NestedTwoPaneViewSplitAcrossHinge(), Navigation),
					GalleryBuilder.NavButton("Open Picture in Picture Window", () => new OpenCompactWindow(), Navigation),
					GalleryBuilder.NavButton("DualScreenInfo with non TwoPaneView", () => new GridUsingDualScreenInfo(), Navigation),
					GalleryBuilder.NavButton("eReader Samples", () => new TwoPage(), Navigation),
				}
			};

			twoPaneView.Pane1 = pane1;
			twoPaneView.Pane2 = pane2;

			Content = twoPaneView;
		}
	}
}
