using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using AProgressBar = Android.Widget.ProgressBar;

namespace Xamarin.Forms.Platform.Android
{
	public class ActivityIndicatorRenderer : ViewRenderer<ActivityIndicator, AProgressBar>
	{
		public ActivityIndicatorRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use ActivityIndicatorRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
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
			if (Element == null || Control == null)
				return;

			Color color = Element.Color;

			if (!color.IsDefault)
				Control.IndeterminateDrawable?.SetColorFilter(color.ToAndroid(), FilterMode.SrcIn);
			else
				Control.IndeterminateDrawable?.ClearColorFilter();
		}

		void UpdateVisibility()
		{
			if (Element == null || Control == null)
				return;

			Control.Visibility = Element.IsRunning ? ViewStates.Visible : ViewStates.Invisible;
		}
	}
}