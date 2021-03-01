using System;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class SliderHandler : AbstractViewHandler<ISlider, UISlider>
	{
		static UIColor? DefaultMinTrackColor;
		static UIColor? DefaultMaxTrackColor;
		static UIColor? DefaultThumbColor;

		protected override UISlider CreateNativeView() => new UISlider { Continuous = true };

		protected override void ConnectHandler(UISlider nativeView)
		{
			nativeView.ValueChanged += OnControlValueChanged;
			nativeView.AddTarget(OnTouchDownControlEvent, UIControlEvent.TouchDown);
			nativeView.AddTarget(OnTouchUpControlEvent, UIControlEvent.TouchUpInside | UIControlEvent.TouchUpOutside);
		}

		protected override void DisconnectHandler(UISlider nativeView)
		{
			nativeView.ValueChanged -= OnControlValueChanged;
			nativeView.RemoveTarget(OnTouchDownControlEvent, UIControlEvent.TouchDown);
			nativeView.RemoveTarget(OnTouchUpControlEvent, UIControlEvent.TouchUpInside | UIControlEvent.TouchUpOutside);
		}

		protected override void SetupDefaults(UISlider nativeView)
		{
			DefaultMinTrackColor = nativeView.MinimumTrackTintColor;
			DefaultMaxTrackColor = nativeView.MaximumTrackTintColor;
			DefaultThumbColor = nativeView.ThumbTintColor;
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
			handler.TypedNativeView?.UpdateMinimumTrackColor(slider, DefaultMinTrackColor);
		}

		public static void MapMaximumTrackColor(SliderHandler handler, ISlider slider)
		{
			handler.TypedNativeView?.UpdateMaximumTrackColor(slider, DefaultMaxTrackColor);
		}

		public static void MapThumbColor(SliderHandler handler, ISlider slider)
		{
			handler.TypedNativeView?.UpdateThumbColor(slider, DefaultThumbColor);
		}

		void OnControlValueChanged(object? sender, EventArgs eventArgs)
		{
			if (TypedNativeView == null || VirtualView == null)
				return;

			VirtualView.Value = TypedNativeView.Value;
		}

		void OnTouchDownControlEvent(object? sender, EventArgs e)
		{
			VirtualView?.DragStarted();
		}

		void OnTouchUpControlEvent(object? sender, EventArgs e)
		{
			VirtualView?.DragCompleted();
		}
	}
}