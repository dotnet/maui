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

using System.Threading;
using System.Threading.Tasks;
using Android.Widget;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	/// <summary>
	/// The successor to IImageSourceHandler, the goal being we can achieve better performance by never creating an Android.Graphics.Bitmap instance
	/// </summary>
	public interface IImageViewHandler : IRegisterable
	{
		Task LoadImageAsync(ImageSource imagesource, ImageView imageView, CancellationToken cancellationToken = default(CancellationToken));
	}
}