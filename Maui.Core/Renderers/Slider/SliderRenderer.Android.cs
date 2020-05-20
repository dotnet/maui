using System.Maui.Core.Controls;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Widget;

namespace System.Maui.Platform
{
	public partial class SliderRenderer : AbstractViewRenderer<ISlider, MauiSlider>
	{
		ColorStateList _defaultProgressTintList;
		ColorStateList _defaultProgressBackgroundTintList;
		PorterDuff.Mode _defaultProgressTintMode;
		PorterDuff.Mode _defaultProgressBackgroundTintMode;
		ColorFilter _defaultThumbColorFilter;

		protected override MauiSlider CreateView()
		{
			var slider = new MauiSlider(Context);

			slider.SetOnSeekBarChangeListener(new MauiSeekBarListener(VirtualView));

			return slider;
		}

		protected override void SetupDefaults()
		{
			base.SetupDefaults();

			var mauiSlider = TypedNativeView;

			if (mauiSlider == null)
				return;

			if (Build.VERSION.SdkInt > BuildVersionCodes.Kitkat)
			{
				_defaultThumbColorFilter = mauiSlider.Thumb.GetColorFilter();
				_defaultProgressTintMode = mauiSlider.ProgressTintMode;
				_defaultProgressBackgroundTintMode = mauiSlider.ProgressBackgroundTintMode;
				_defaultProgressTintList = mauiSlider.ProgressTintList;
				_defaultProgressBackgroundTintList = mauiSlider.ProgressBackgroundTintList;
			}
		}

		public static void MapPropertyMinimum(IViewRenderer renderer, ISlider slider)
		{
			if (!(renderer.NativeView is MauiSlider mauiSlider))
				return;

			mauiSlider.Min = (int)slider.Minimum;
		}

		public static void MapPropertyMaximum(IViewRenderer renderer, ISlider slider)
		{
			if (!(renderer.NativeView is MauiSlider mauiSlider))
				return;

			mauiSlider.Max = (int)slider.Maximum;
		}

		public static void MapPropertyValue(IViewRenderer renderer, ISlider slider)
		{
			if (!(renderer.NativeView is MauiSlider mauiSlider))
				return;

			mauiSlider.Progress = (int)slider.Value;
		}

		public static void MapPropertyMinimumTrackColor(IViewRenderer renderer, ISlider slider)
		{
			if (!(renderer is SliderRenderer sliderRenderer) || !(renderer.NativeView is MauiSlider mauiSlider))
				return;

			if (slider.MinimumTrackColor == Color.Default)
			{
				mauiSlider.ProgressTintList = sliderRenderer._defaultProgressTintList;
				mauiSlider.ProgressTintMode = sliderRenderer._defaultProgressTintMode;
			}
			else
			{
				mauiSlider.ProgressTintList = ColorStateList.ValueOf(slider.MinimumTrackColor.ToNative());
				mauiSlider.ProgressTintMode = PorterDuff.Mode.SrcIn;
			}
		}

		public static void MapPropertyMaximumTrackColor(IViewRenderer renderer, ISlider slider)
		{
			if (!(renderer is SliderRenderer sliderRenderer) || !(renderer.NativeView is MauiSlider mauiSlider))
				return;

				if (slider.MaximumTrackColor == Color.Default)
				{
					mauiSlider.ProgressBackgroundTintList = sliderRenderer._defaultProgressBackgroundTintList;
					mauiSlider.ProgressBackgroundTintMode = sliderRenderer._defaultProgressBackgroundTintMode;
				}
				else
				{
					mauiSlider.ProgressBackgroundTintList = ColorStateList.ValueOf(slider.MaximumTrackColor.ToNative());
					mauiSlider.ProgressBackgroundTintMode = PorterDuff.Mode.SrcIn;
				}
		}

		public static void MapPropertyThumbColor(IViewRenderer renderer, ISlider slider)
		{
			if (!(renderer is SliderRenderer sliderRenderer) || !(renderer.NativeView is MauiSlider mauiSlider))
				return;

			mauiSlider.Thumb.SetColorFilter(slider.ThumbColor, sliderRenderer._defaultThumbColorFilter, FilterMode.SrcIn);
		}
		
		internal class MauiSeekBarListener : Java.Lang.Object, SeekBar.IOnSeekBarChangeListener
		{
			readonly ISlider _virtualView;

			public MauiSeekBarListener(ISlider virtualView)
			{
				_virtualView = virtualView;
			}

			public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
			{
				_virtualView.Value = progress;
			}

			public void OnStartTrackingTouch(SeekBar seekBar)
			{
				_virtualView.DragStarted();
			}

			public void OnStopTrackingTouch(SeekBar seekBar)
			{
				_virtualView.DragCompleted();
			}
		}
	}
}