using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using AImageView = Android.Widget.ImageView;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class CountedImageHandler
	{
		protected override AImageView CreatePlatformView() => new CountedImageView(Context);

		public List<(string Member, object Value)> ImageEvents => ((CountedImageView)PlatformView).ImageEvents;

		public class CountedImageView : AImageView
		{
			public CountedImageView(Context context)
				: base(context)
			{
			}

			public List<(string, object)> ImageEvents { get; } = new List<(string, object)>();

			public override void SetImageBitmap(Bitmap bm)
			{
				base.SetImageBitmap(bm);
				Log(bm);
			}

			public override void SetImageDrawable(Drawable drawable)
			{
				base.SetImageDrawable(drawable);
				Log(drawable);
			}

			[System.Runtime.Versioning.SupportedOSPlatform("android23.0")]
			public override void SetImageIcon(Icon icon)
			{
				base.SetImageIcon(icon);
				Log(icon);
			}

			public override void SetImageResource(int resId)
			{
				base.SetImageResource(resId);
				Log(resId);
			}

			public override void SetImageURI(Android.Net.Uri uri)
			{
				base.SetImageURI(uri);
				Log(uri);
			}

			void Log(object value, [CallerMemberName] string member = null)
			{
				ImageEvents.Add((member, value));
			}
		}
	}
}