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


namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public interface IImageVisualElementRenderer : IVisualNativeElementRenderer
	{
		bool IsDisposed { get; }
		void SetImage(Microsoft.UI.Xaml.Media.ImageSource image);
		Microsoft.UI.Xaml.Controls.Image GetImage();
	}
}
