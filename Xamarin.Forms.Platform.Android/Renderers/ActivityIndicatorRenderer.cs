using System.ComponentModel;
using Android.Graphics;
using Android.OS;
using Android.Views;
using AProgressBar = Android.Widget.ProgressBar;

namespace Xamarin.Forms.Platform.Android
{
	public class ActivityIndicatorRenderer : ViewRenderer<ActivityIndicator, AProgressBar>
	{
		public ActivityIndicatorRenderer()
		{
			AutoPackage = false;
		}

		protected override AProgressBar CreateNativeControl()
		{
			return new AProgressBar(Context) { Indeterminate = true };
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ActivityIndicator> e)
		{
			base.OnElementChanged(e);

			AProgressBar progressBar = Control;
			if (progressBar == null)
			{
				progressBar = CreateNativeControl();
				SetNativeControl(progressBar);
			}

			UpdateColor();
			UpdateVisibility();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == ActivityIndicator.IsRunningProperty.PropertyName)
				UpdateVisibility();
			else if (e.PropertyName == ActivityIndicator.ColorProperty.PropertyName)
				UpdateColor();
		}

		void UpdateColor()
		{
			Color color = Element.Color;

			if (!color.IsDefault)
				Control.IndeterminateDrawable.SetColorFilter(color.ToAndroid(), PorterDuff.Mode.SrcIn);
			else
				Control.IndeterminateDrawable.ClearColorFilter();
		}

		void UpdateVisibility()
		{
			Control.Visibility = Element.IsRunning ? ViewStates.Visible : ViewStates.Invisible;
		}
	}
}