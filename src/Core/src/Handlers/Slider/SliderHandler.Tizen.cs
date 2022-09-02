using System;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Tizen.NUI.Components;

namespace Microsoft.Maui.Handlers
{
	public partial class SliderHandler : ViewHandler<ISlider, Slider>
	{
		protected override Slider CreatePlatformView() => new Slider
		{
			Focusable = true,
		};

		protected override void ConnectHandler(Slider platformView)
		{
			platformView.ValueChanged += OnControlValueChanged;
			platformView.SlidingStarted += OnSlidingStarted;
			platformView.SlidingFinished += OnSlidingFinished;
		}

		void OnSlidingStarted(object? sender, SliderSlidingStartedEventArgs e)
		{
			VirtualView.DragStarted();
		}

		void OnSlidingFinished(object? sender, SliderSlidingFinishedEventArgs e)
		{
			VirtualView.DragCompleted();
		}

		protected override void DisconnectHandler(Slider platformView)
		{
			if (!platformView.HasBody())
				return;

			platformView.ValueChanged -= OnControlValueChanged;
			platformView.SlidingStarted -= OnSlidingStarted;
			platformView.SlidingFinished -= OnSlidingFinished;
		}

		public static void MapMinimum(ISliderHandler handler, ISlider slider)
		{
			handler.PlatformView?.UpdateMinimum(slider);
		}

		public static void MapMaximum(ISliderHandler handler, ISlider slider)
		{
			handler.PlatformView?.UpdateMaximum(slider);
		}

		public static void MapValue(ISliderHandler handler, ISlider slider)
		{
			handler.PlatformView?.UpdateValue(slider);
		}

		public static void MapMinimumTrackColor(ISliderHandler handler, ISlider slider)
		{
			handler.PlatformView?.UpdateMinimumTrackColor(slider);
		}

		public static void MapMaximumTrackColor(ISliderHandler handler, ISlider slider)
		{
			handler.PlatformView?.UpdateMaximumTrackColor(slider);
		}

		public static void MapThumbColor(ISliderHandler handler, ISlider slider)
		{
			handler.PlatformView?.UpdateThumbColor(slider);
		}

		public static void MapThumbImageSource(ISliderHandler handler, ISlider slider)
		{
			var provider = handler.GetRequiredService<IImageSourceServiceProvider>();

			handler.PlatformView?.UpdateThumbImageSourceAsync(slider, provider)
				.FireAndForget(handler);
		}

		void OnControlValueChanged(object? sender, EventArgs eventArgs)
		{
			if (PlatformView == null || VirtualView == null || PlatformView.CurrentValue == VirtualView.Value)
				return;

			VirtualView.Value = PlatformView.CurrentValue;
		}
	}
}