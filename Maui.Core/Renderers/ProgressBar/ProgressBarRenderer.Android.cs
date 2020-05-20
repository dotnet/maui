using Android.Content.Res;
using Android.OS;
using AProgressBar = Android.Widget.ProgressBar;

namespace System.Maui.Platform
{
	public partial class ProgressBarRenderer : AbstractViewRenderer<IProgress, AProgressBar>
	{
		protected override AProgressBar CreateView()
		{
			return new AProgressBar(Context, null, Android.Resource.Attribute.ProgressBarStyleHorizontal) { Indeterminate = false, Max = 10000 };
		}

		public static void MapPropertyProgress(IViewRenderer renderer, IProgress progressBar)
		{
			if (!(renderer.NativeView is AProgressBar aProgressBar))
				return;

			aProgressBar.Progress = (int)(progressBar.Progress * 10000);
		}

		public static void MapPropertyProgressColor(IViewRenderer renderer, IProgress progressBar)
		{
			if (!(renderer.NativeView is AProgressBar aProgressBar))
				return;


			Color color = progressBar.ProgressColor;

			if (color.IsDefault)
			{
				(aProgressBar.Indeterminate ? aProgressBar.IndeterminateDrawable :
					aProgressBar.ProgressDrawable).ClearColorFilter();
			}
			else
			{
				if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
				{
					(aProgressBar.Indeterminate ? aProgressBar.IndeterminateDrawable :
						aProgressBar.ProgressDrawable).SetColorFilter(color, FilterMode.SrcIn);
				}
				else
				{
					var tintList = ColorStateList.ValueOf(color.ToNative());
					if (aProgressBar.Indeterminate)
						aProgressBar.IndeterminateTintList = tintList;
					else
						aProgressBar.ProgressTintList = tintList;
				}
			}
		}
	}
}