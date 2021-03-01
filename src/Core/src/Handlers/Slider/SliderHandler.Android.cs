using Android.Content.Res;
using Android.Graphics;
using Android.Widget;

namespace Microsoft.Maui.Handlers
{
	public partial class SliderHandler : AbstractViewHandler<ISlider, SeekBar>
	{
		static ColorStateList? DefaultProgressTintList { get; set; }
		static ColorStateList? DefaultProgressBackgroundTintList { get; set; }
		static PorterDuff.Mode? DefaultProgressTintMode { get; set; }
		static PorterDuff.Mode? DefaultProgressBackgroundTintMode { get; set; }
		static ColorFilter? DefaultThumbColorFilter { get; set; }

		SeekBarChangeListener ChangeListener { get; } = new SeekBarChangeListener();

		protected override SeekBar CreateNativeView()
		{
			return new SeekBar(Context)
			{
				DuplicateParentStateEnabled = false,
				Max = (int)SliderExtensions.NativeMaxValue
			};
		}

		protected override void ConnectHandler(SeekBar nativeView)
		{
			ChangeListener.Handler = this;
			nativeView.SetOnSeekBarChangeListener(ChangeListener);
		}

		protected override void DisconnectHandler(SeekBar nativeView)
		{
			ChangeListener.Handler = null;
			nativeView.SetOnSeekBarChangeListener(null);
		}

		protected override void SetupDefaults(SeekBar nativeView)
		{
			DefaultThumbColorFilter = nativeView.Thumb?.GetColorFilter();
			DefaultProgressTintMode = nativeView.ProgressTintMode;
			DefaultProgressBackgroundTintMode = nativeView.ProgressBackgroundTintMode;
			DefaultProgressTintList = nativeView.ProgressTintList;
			DefaultProgressBackgroundTintList = nativeView.ProgressBackgroundTintList;
		}

		public static void MapMinimum(SliderHandler handler, ISlider slider)
		{
			handler.TypedNativeView?.UpdateMinimum(slider);
		}

		public static void MapMaximum(SliderHandler handler, ISlider slider)
		{
			handler.TypedNativeView?.UpdateMaximum(slider);
		}

		public static void MapValue(SliderHandler handler, ISlider slider)
		{
			handler.TypedNativeView?.UpdateValue(slider);
		}

		public static void MapMinimumTrackColor(SliderHandler handler, ISlider slider)
		{
			handler.TypedNativeView?.UpdateMinimumTrackColor(slider, DefaultProgressBackgroundTintList, DefaultProgressBackgroundTintMode);
		}

		public static void MapMaximumTrackColor(SliderHandler handler, ISlider slider)
		{
			handler.TypedNativeView?.UpdateMaximumTrackColor(slider, DefaultProgressTintList, DefaultProgressTintMode);
		}

		public static void MapThumbColor(SliderHandler handler, ISlider slider)
		{
			handler.TypedNativeView?.UpdateThumbColor(slider, DefaultThumbColorFilter);
		}

		void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
		{
			if (VirtualView == null || !fromUser)
				return;

			var min = VirtualView.Minimum;
			var max = VirtualView.Maximum;

			var value = min + (max - min) * (progress / SliderExtensions.NativeMaxValue);

			VirtualView.Value = value;
		}

		void OnStartTrackingTouch(SeekBar seekBar) =>
			VirtualView?.DragStarted();

		void OnStopTrackingTouch(SeekBar seekBar) =>
			VirtualView?.DragCompleted();

		internal class SeekBarChangeListener : Java.Lang.Object, SeekBar.IOnSeekBarChangeListener
		{
			public SliderHandler? Handler { get; set; }

			public SeekBarChangeListener()
			{
			}

			public void OnProgressChanged(SeekBar? seekBar, int progress, bool fromUser)
			{
				if (Handler == null || seekBar == null)
					return;

				Handler.OnProgressChanged(seekBar, progress, fromUser);
			}

			public void OnStartTrackingTouch(SeekBar? seekBar)
			{
				if (Handler == null || seekBar == null)
					return;

				Handler.OnStartTrackingTouch(seekBar);
			}

			public void OnStopTrackingTouch(SeekBar? seekBar)
			{
				if (Handler == null || seekBar == null)
					return;

				Handler.OnStopTrackingTouch(seekBar);
			}
		}
	}
}