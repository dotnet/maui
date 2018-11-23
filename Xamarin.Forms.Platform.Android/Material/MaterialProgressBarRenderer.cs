#if __ANDROID81__
#else
using System;
using System.ComponentModel;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.View;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android.Material;
using AProgressBar = Android.Widget.ProgressBar;
using AViewCompat = Android.Support.V4.View.ViewCompat;

[assembly: ExportRenderer(typeof(Xamarin.Forms.ProgressBar), typeof(MaterialProgressBarRenderer), new[] { typeof(VisualRendererMarker.Material) })]
namespace Xamarin.Forms.Platform.Android.Material
{
	public class MaterialProgressBarRenderer : ProgressBarRenderer
	{
		public MaterialProgressBarRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		protected override AProgressBar CreateNativeControl()
		{
			if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
			{
				return new AProgressBar(new ContextThemeWrapper(Context, Resource.Style.XamarinFormsMaterialProgressBarHorizontal), null, Resource.Style.XamarinFormsMaterialProgressBarHorizontal)
				{
					Indeterminate = false,
					Max = 10000,

				};
			}

			return base.CreateNativeControl();

		}

		protected override void UpdateBackgroundColor()
		{
			if (Control == null)
				return;

			var color = Element.BackgroundColor;
			if (color != Color.Default)
				Control.ProgressBackgroundTintList = ColorStateList.ValueOf(color.ToAndroid());
		}

		internal protected override void UpdateProgressColor()
		{
			if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
			{
				base.UpdateProgressColor();
				return;
			}

			if (Element == null || Control == null)
				return;

			Color color = Element.ProgressColor;
			if (color == Color.Default)
				return;

			var tintList = ColorStateList.ValueOf(color.ToAndroid());
			if (Control.Indeterminate)
				Control.IndeterminateTintList = tintList;
			else
				Control.ProgressTintList = tintList;

		}

		protected override void OnElementChanged(ElementChangedEventArgs<ProgressBar> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
				UpdateBackgroundColor();
		}
	}
}
#endif