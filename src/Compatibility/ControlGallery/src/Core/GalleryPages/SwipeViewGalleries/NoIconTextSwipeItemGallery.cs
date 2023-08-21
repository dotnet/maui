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
using Microsoft.Maui.Graphics;
namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.SwipeViewGalleries
{
	public class NoIconTextSwipeItemGallery : ContentPage
	{
		public NoIconTextSwipeItemGallery()
		{
			Title = "No Icon or Text SwipeItem Gallery";

			var swipeLayout = new StackLayout
			{
				Margin = new Thickness(12)
			};

			var noIconSwipeItem = new SwipeItem
			{
				BackgroundColor = Colors.Red,
				Text = "File"
			};

			var noTextSwipeItem = new SwipeItem
			{
				BackgroundColor = Colors.BlueViolet,
				IconImageSource = "calculator.png"
			};

			var swipeItems = new SwipeItems { noIconSwipeItem, noTextSwipeItem };

			swipeItems.Mode = SwipeMode.Reveal;

			var swipeContent = new Grid
			{
				BackgroundColor = Colors.Gray
			};

			var swipeLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe to Right (File)"
			};

			swipeContent.Children.Add(swipeLabel);

			var swipeView = new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = swipeItems,
				Content = swipeContent
			};

			swipeLayout.Children.Add(swipeView);

			Content = swipeLayout;
		}
	}
}