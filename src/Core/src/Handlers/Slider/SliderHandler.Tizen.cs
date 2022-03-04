using System;
using Tizen.UIExtensions.NUI.GraphicsView;

namespace Microsoft.Maui.Handlers
{
	public partial class SliderHandler : ViewHandler<ISlider, Slider>
	{
		protected override Slider CreatePlatformView() => new Slider
		{
			Focusable = true,
		};

		protected override void ConnectHandler(Slider nativeView)
		{
			nativeView!.ValueChanged += OnControlValueChanged;
		}

		protected override void DisconnectHandler(Slider nativeView)
		{
			nativeView!.ValueChanged -= OnControlValueChanged;
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

		[MissingMapper]
		public static void MapThumbImageSource(ISliderHandler handler, ISlider slider) { }

		void OnControlValueChanged(object? sender, EventArgs eventArgs)
		{
			if (PlatformView == null || VirtualView == null)
				return;

			VirtualView.DragStarted();
			VirtualView.Value = PlatformView.Value;
			VirtualView.DragCompleted();
		}
	}
}