using UIKit;

namespace System.Maui.Platform
{
	public partial class SliderRenderer : AbstractViewRenderer<ISlider, UISlider>
	{
		UIColor _defaultMinTrackColor;
		UIColor _defaultMaxTrackColor;
		UIColor _defaultThumbColor;

		protected override UISlider CreateView()
		{
			var slider = new UISlider();

			UpdateDefaultColors(slider);

			slider.ValueChanged += OnControlValueChanged;

			slider.AddTarget(OnTouchDownControlEvent, UIControlEvent.TouchDown);
			slider.AddTarget(OnTouchUpControlEvent, UIControlEvent.TouchUpInside | UIControlEvent.TouchUpOutside);

			return slider;
		}

		protected override void DisposeView(UISlider slider)
		{
			slider.ValueChanged -= OnControlValueChanged;
			slider.RemoveTarget(OnTouchDownControlEvent, UIControlEvent.TouchDown);
			slider.RemoveTarget(OnTouchUpControlEvent, UIControlEvent.TouchUpInside | UIControlEvent.TouchUpOutside);

			base.DisposeView(slider);
		}

		public static void MapPropertyMinimum(IViewRenderer renderer, ISlider slider)
		{
			if (!(renderer.NativeView is UISlider uISlider))
				return;

			uISlider.MinValue = (float)slider.Minimum;
		}

		public static void MapPropertyMaximum(IViewRenderer renderer, ISlider slider)
		{
			if (!(renderer.NativeView is UISlider uISlider))
				return;

			uISlider.MaxValue = (float)slider.Maximum;
		}

		public static void MapPropertyValue(IViewRenderer renderer, ISlider slider)
		{
			if (!(renderer.NativeView is UISlider uISlider))
				return;

			if ((float)slider.Value != uISlider.Value)
				uISlider.Value = (float)slider.Value;
		}

		public static void MapPropertyMinimumTrackColor(IViewRenderer renderer, ISlider slider)
		{
			if (!(renderer is SliderRenderer sliderRenderer) || !(renderer.NativeView is UISlider uISlider))
				return;

			if (slider.MinimumTrackColor == Color.Default)
				uISlider.MinimumTrackTintColor = sliderRenderer._defaultMinTrackColor;
			else
				uISlider.MinimumTrackTintColor = slider.MinimumTrackColor.ToNativeColor();
		}

		public static void MapPropertyMaximumTrackColor(IViewRenderer renderer, ISlider slider)
		{
			if (!(renderer is SliderRenderer sliderRenderer) || !(renderer.NativeView is UISlider uISlider))
				return;

			if (slider.MaximumTrackColor == Color.Default)
				uISlider.MaximumTrackTintColor = sliderRenderer._defaultMaxTrackColor;
			else
				uISlider.MaximumTrackTintColor = slider.MaximumTrackColor.ToNativeColor();
		}

		public static void MapPropertyThumbColor(IViewRenderer renderer, ISlider slider)
		{
			if (!(renderer is SliderRenderer sliderRenderer) || !(renderer.NativeView is UISlider uISlider))
				return;

			if (slider.ThumbColor == Color.Default)
				uISlider.ThumbTintColor = sliderRenderer._defaultThumbColor;
			else
				uISlider.ThumbTintColor = slider.ThumbColor.ToNativeColor();
		}

		void UpdateDefaultColors(UISlider uISlider)
		{
			_defaultMinTrackColor = uISlider.MinimumTrackTintColor;
			_defaultMaxTrackColor = uISlider.MaximumTrackTintColor;
			_defaultThumbColor = uISlider.ThumbTintColor;
		}

		void OnControlValueChanged(object sender, EventArgs eventArgs)
		{
			VirtualView.Value = TypedNativeView.Value;
		}

		void OnTouchDownControlEvent(object sender, EventArgs e)
		{
			VirtualView.DragStarted();
		}

		void OnTouchUpControlEvent(object sender, EventArgs e)
		{
			VirtualView.DragCompleted();
		}
	}
}