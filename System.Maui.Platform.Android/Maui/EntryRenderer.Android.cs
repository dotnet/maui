using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;

#if __ANDROID_29__
using AndroidX.Core.View;
using AndroidX.AppCompat.Widget;
#else
using Android.Support.V4.View;
using Android.Support.V7.Widget;
#endif

using AColor = Android.Graphics.Color;
using AView = Android.Views.View;
using System.Maui.Platform;
using System.Maui.Platform.Android;
using Android.Widget;

namespace System.Maui.Platform
{
	public partial class EntryRenderer : AbstractViewRenderer<ITextInput, AppCompatEditText>
	{
		public EntryRenderer(PropertyMapper mapper) : base(mapper)
		{
		}

		protected override AppCompatEditText CreateView()
		{
			return new AppCompatEditText(Context);
		}

		static void MapPropertyText(IMauiRenderer arg1, ITextInput arg2)
		{
			System.Maui.Platform.Android.EntryRenderer
				.UpdateText(arg2, arg1.NativeView as EditText);
		}
	}
}
