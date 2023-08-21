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
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ButtonBorderBackgroundGalleryPage : ContentPage
	{
		public ButtonBorderBackgroundGalleryPage()
			: this(VisualMarker.MatchParent)
		{
		}

		public ButtonBorderBackgroundGalleryPage(IVisual visual)
		{
			InitializeComponent();
			Visual = visual;

			// buttons are transparent on default iOS, so we have to give them something
			if (DeviceInfo.Platform == DevicePlatform.iOS)
			{
				if (Visual != VisualMarker.Material)
				{
					SetBackground(Content);

					void SetBackground(View view)
					{
						if (view is Button button && !button.IsSet(Button.BackgroundColorProperty))
							view.BackgroundColor = Colors.LightGray;

						if (view is Layout layout)
						{
							foreach (var child in layout.Children)
							{
								if (child is View childView)
									SetBackground(childView);
							}
						}
					}
				}
			}
		}

		void HandleChecks_Clicked(object sender, System.EventArgs e)
		{
			var thisButton = sender as Button;
			var layout = thisButton.Parent as Layout;
			foreach (var child in layout.Children)
			{
				var button = child as Button;

				Console.WriteLine($"{button.Text} => {button.Bounds}");
			}
		}
	}
}
