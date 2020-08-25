using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;

using AndroidX.Core.View;
using AndroidX.AppCompat.Widget;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;
using System.Maui.Platform;
using Android.Widget;
using Xamarin.Forms.Platform.Android;

namespace System.Maui.Platform
{
	public partial class EntryRenderer : AbstractViewRenderer<ITextInput, FormsEditText>
	{
		public EntryRenderer(PropertyMapper mapper) : base(mapper)
		{
		}

		protected override FormsEditText CreateView()
		{
			return new FormsEditText(Context);
		}

		static void MapPropertyText(IViewRenderer arg1, ITextInput arg2)
		{
			FormsEditText.UpdateText(arg2, arg1.NativeView as EditText);
		}
	}
}
