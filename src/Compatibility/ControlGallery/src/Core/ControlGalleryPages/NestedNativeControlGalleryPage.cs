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

namespace Microsoft.Maui.Controls.ControlGallery
{
	public partial class NestedNativeControlGalleryPage : ContentPage
	{
		public new StackLayout Layout { get; set; }

		public bool NativeControlsAdded { get; set; }

		public const string ReadyForNativeControlsMessage = "ReadyForNativeControls";

		protected override void OnAppearing()
		{
			base.OnAppearing();
			MessagingCenter.Send(this, ReadyForNativeControlsMessage);
		}

		public NestedNativeControlGalleryPage()
		{
			Layout = new StackLayout { Padding = 20, VerticalOptions = LayoutOptions.FillAndExpand };

			Content = new ScrollView { Content = Layout };

			var label = new Label { Text = "There should be some native controls right below this", FontSize = 12 };

			var testLabel = new Label { Text = "Forms Label", FontSize = 14 };
			var button = new Button { Text = "Resize Forms Label", HeightRequest = 80 };
			double originalSize = testLabel.FontSize;
			button.Clicked += (sender, args) => { testLabel.FontSize = testLabel.FontSize == originalSize ? 24 : 14; };

			Layout.Children.Add(testLabel);
			Layout.Children.Add(button);
			Layout.Children.Add(label);
		}
	}
}