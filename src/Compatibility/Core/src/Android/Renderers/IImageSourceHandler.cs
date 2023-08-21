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
using Android.Content;
using Android.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public interface IImageSourceHandler : IRegisterable
	{
		Task<Bitmap> LoadImageAsync(ImageSource imagesource, Context context, CancellationToken cancelationToken = default(CancellationToken));
	}
}