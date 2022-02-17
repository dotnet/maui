using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class SliderHandler : ViewHandler<ISlider, UISlider>
	{
		static UIColor? DefaultMinTrackColor;
		static UIColor? DefaultMaxTrackColor;
		static UIColor? DefaultThumbColor;

		protected override UISlider CreatePlatformView() => new UISlider { Continuous = true };

		protected override void ConnectHandler(UISlider platformView)
		{
			base.ConnectHandler(platformView);

			platformView.ValueChanged += OnControlValueChanged;
			platformView.AddTarget(OnTouchDownControlEvent, UIControlEvent.TouchDown);
			platformView.AddTarget(OnTouchUpControlEvent, UIControlEvent.TouchUpInside | UIControlEvent.TouchUpOutside);
		}

		protected override void DisconnectHandler(UISlider platformView)
		{
			base.DisconnectHandler(platformView);

			platformView.ValueChanged -= OnControlValueChanged;
			platformView.RemoveTarget(OnTouchDownControlEvent, UIControlEvent.TouchDown);
			platformView.RemoveTarget(OnTouchUpControlEvent, UIControlEvent.TouchUpInside | UIControlEvent.TouchUpOutside);
		}

		void SetupDefaults(UISlider platformView)
		{
			DefaultMinTrackColor = platformView.MinimumTrackTintColor;
			DefaultMaxTrackColor = platformView.MaximumTrackTintColor;
			DefaultThumbColor = platformView.ThumbTintColor;
		}

		public static void MapMinimum(SliderHandler handler, ISlider slider)
		{
			handler.PlatformView?.UpdateMinimum(slider);
		}

		public static void MapMaximum(SliderHandler handler, ISlider slider)
		{
			handler.PlatformView?.UpdateMaximum(slider);
		}

		public static void MapValue(SliderHandler handler, ISlider slider)
		{
			handler.PlatformView?.UpdateValue(slider);
		}

		public static void MapMinimumTrackColor(SliderHandler handler, ISlider slider)
		{
			handler.PlatformView?.UpdateMinimumTrackColor(slider, DefaultMinTrackColor);
		}

		public static void MapMaximumTrackColor(SliderHandler handler, ISlider slider)
		{
			handler.PlatformView?.UpdateMaximumTrackColor(slider, DefaultMaxTrackColor);
		}

		public static void MapThumbColor(SliderHandler handler, ISlider slider)
		{
			handler.PlatformView?.UpdateThumbColor(slider, DefaultThumbColor);
		}

		public static void MapThumbImageSource(SliderHandler handler, ISlider slider)
		{
			var provider = handler.GetRequiredService<IImageSourceServiceProvider>();

			handler.PlatformView?.UpdateThumbImageSourceAsync(slider, provider)
				.FireAndForget(handler);
		}

		void OnControlValueChanged(object? sender, EventArgs eventArgs)
		{
			if (PlatformView == null || VirtualView == null)
				return;

			VirtualView.Value = PlatformView.Value;
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