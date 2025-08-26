using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text;

namespace Microsoft.Maui.Platform
{
	internal class ImageGetter : Java.Lang.Object, Html.IImageGetter
	{
		private static readonly Dictionary<string, BitmapDrawable> Bitmaps = new();
		private readonly Action? _updateTextHtml;

		public ImageGetter() { }

		public ImageGetter(Action updateTextHtml)
		{
			_updateTextHtml = updateTextHtml;
		}

		public Drawable? GetDrawable(string? source)
		{
			if (string.IsNullOrWhiteSpace(source))
			{
				return null;
			}

			if (Bitmaps.TryGetValue(source, out BitmapDrawable? bitmap))
			{
				return bitmap;
			}

			LoadImageAsync(source);
			return new ColorDrawable(Color.Transparent);
		}

		private async void LoadImageAsync(string source)
		{
			try
			{
				using HttpClient httpClient = new HttpClient();
				using Stream stream = await httpClient.GetStreamAsync(source);
				Bitmap? bitmap = await BitmapFactory.DecodeStreamAsync(stream);
				if (bitmap != null)
				{
#pragma warning disable CS0618 // Type or member is obsolete
					var drawable = new BitmapDrawable(bitmap);
#pragma warning restore CS0618 // Type or member is obsolete
					drawable.SetBounds(0, 0, bitmap.Width, bitmap.Height);
					Bitmaps[source] = drawable;
					_updateTextHtml?.Invoke();
				}
			}
			catch { }
		}
	}
}

