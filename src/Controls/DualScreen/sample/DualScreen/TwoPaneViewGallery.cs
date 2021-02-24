using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.DualScreen;

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
				MinTallModeHeight = 0,
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
					NavButton("Lots of TwoPaneViews", () => new TwoPaneViewBonanza(), Navigation),
				}
			};

			var pane2 = new StackLayout
			{
				Children =
				{
					NavButton("Nested TwoPaneView Split Across Hinge", () => new NestedTwoPaneViewSplitAcrossHinge(), Navigation),
					NavButton("DualScreenInfo with non TwoPaneView", () => new GridUsingDualScreenInfo(), Navigation),
					NavButton("eReader Samples", () => new TwoPage(), Navigation),
					NavButton("Dual Screen Info Samples", () => new DualScreenInfoGallery(), Navigation),
					StyleButton(new Button(){ Text = "Collect Garbage", Command = new Command(OnCollectGarbage) })
				}
			};

			twoPaneView.Pane1 = pane1;
			twoPaneView.Pane2 = pane2;

			Content = twoPaneView;
		}

		void OnCollectGarbage(object obj)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		public static Button StyleButton(Button button)
		{
			button.FontSize = 10;
			button.HeightRequest = Device.RuntimePlatform == Device.Android ? 40 : 30;

			return button;
		}

		public static Button NavButton(string galleryName, Func<Page> gallery, INavigation nav)
		{
			var automationId = System.Text.RegularExpressions.Regex.Replace(galleryName, " |\\(|\\)", string.Empty);
			var button = StyleButton(new Button { Text = $"{galleryName}", AutomationId = automationId });
			button.Clicked += (sender, args) => { nav.PushAsync(gallery()); };
			return button;
		}
	}
}
