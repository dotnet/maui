using System;
using System.Collections.Generic;
using System.Text;
using System.Maui;
using System.Maui.DualScreen;

namespace DualScreen
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

			var twoPaneView = new TwoPaneView()
			{
				MinTallModeHeight  = 0,
				MinWideModeWidth = 0
			};

			var pane1 = new StackLayout
			{
				Children =
				{
					NavButton("TwoPanePropertiesGallery", () => new TwoPanePropertiesGallery(), Navigation),
					NavButton("Master Details Sample", () => new MasterDetail(), Navigation),
					NavButton("Companion Pane", () => new CompanionPane(), Navigation),
					NavButton("ExtendCanvas Sample", () => new ExtendCanvas(), Navigation),
					NavButton("DualViewMapPage Sample", () => new DualViewMapPage(), Navigation),
				}
			};

			var pane2 = new StackLayout
			{
				Children =
				{
					NavButton("Nested TwoPaneView Split Across Hinge", () => new NestedTwoPaneViewSplitAcrossHinge(), Navigation),
					NavButton("Open Picture in Picture Window", () => new OpenCompactWindow(), Navigation),
					NavButton("DualScreenInfo with non TwoPaneView", () => new GridUsingDualScreenInfo(), Navigation),
					NavButton("eReader Samples", () => new TwoPage(), Navigation),
					NavButton("Dual Screen Info Samples", () => new DualScreenInfoGallery(), Navigation),
				}
			};

			twoPaneView.Pane1 = pane1;
			twoPaneView.Pane2 = pane2;

			Content = twoPaneView;
		}

		public static Button NavButton(string galleryName, Func<Page> gallery, INavigation nav)
		{
			var automationId = System.Text.RegularExpressions.Regex.Replace(galleryName, " |\\(|\\)", string.Empty);
			var button = new Button { Text = $"{galleryName}", AutomationId = automationId, FontSize = 10, HeightRequest = Device.RuntimePlatform == Device.Android ? 40 : 30 };
			button.Clicked += (sender, args) => { nav.PushAsync(gallery()); };
			return button;
		}
	}
}
