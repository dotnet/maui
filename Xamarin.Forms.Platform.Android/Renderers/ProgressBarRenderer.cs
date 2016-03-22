using System.ComponentModel;
using AProgressBar = Android.Widget.ProgressBar;

namespace Xamarin.Forms.Platform.Android
{
	public class ProgressBarRenderer : ViewRenderer<ProgressBar, AProgressBar>
	{
		public ProgressBarRenderer()
		{
			AutoPackage = false;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ProgressBar> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement == null)
			{
				var progressBar = new AProgressBar(Context, null, global::Android.Resource.Attribute.ProgressBarStyleHorizontal) { Indeterminate = false, Max = 10000 };

				SetNativeControl(progressBar);
			}

			UpdateProgress();
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