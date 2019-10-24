using System;
using System.ComponentModel;
using Android.Content;
using AProgressBar = Android.Widget.ProgressBar;

namespace Xamarin.Forms.Platform.Android
{
	public class ProgressBarRenderer : ViewRenderer<ProgressBar, AProgressBar>
	{
		public ProgressBarRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use ProgressBarRenderer(Context) instead.")]
		public ProgressBarRenderer()
		{
			AutoPackage = false;
		}

		protected override AProgressBar CreateNativeControl()
		{
			return new AProgressBar(Context, null, global::Android.Resource.Attribute.ProgressBarStyleHorizontal) { Indeterminate = false, Max = 10000 };
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ProgressBar> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var progressBar = CreateNativeControl();

					SetNativeControl(progressBar);
				}
				UpdateProgress();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == ProgressBar.ProgressProperty.PropertyName)
				UpdateProgress();
		}

		void UpdateProgress()
		{
			Control.Progress = (int)(Element.Progress * 10000);
		}
	}
}