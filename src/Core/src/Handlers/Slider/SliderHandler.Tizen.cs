using System;
using Tizen.UIExtensions.ElmSharp;
using EColor = ElmSharp.Color;
using ESlider = ElmSharp.Slider;

namespace Microsoft.Maui.Handlers
{
	public partial class SliderHandler : ViewHandler<ISlider, ESlider>
	{
		static EColor? DefaultMinTrackColor;
		static EColor? DefaultMaxTrackColor;
		static EColor? DefaultThumbColor;

		protected override ESlider CreatePlatformView() => new ESlider(NativeParent);

		protected override void ConnectHandler(ESlider platformView)
		{
			platformView!.ValueChanged += OnControlValueChanged;
			platformView!.DragStarted += OnDragStarted;
			platformView!.DragStopped += OnDragStopped;
		}

		protected override void DisconnectHandler(ESlider platformView)
		{
			platformView!.ValueChanged -= OnControlValueChanged;
			platformView!.DragStarted -= OnDragStarted;
			platformView!.DragStopped -= OnDragStopped;
		}

		void SetupDefaults(ESlider platformView)
		{
			DefaultMinTrackColor = platformView.GetBarColor();
			DefaultMaxTrackColor = platformView.GetBackgroundColor();
			DefaultThumbColor = platformView.GetHandlerColor();
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
			handler.PlatformView?.UpdateMinimumTrackColor(slider, DefaultMinTrackColor);
		}

		public static void MapMaximumTrackColor(ISliderHandler handler, ISlider slider)
		{
			handler.PlatformView?.UpdateMaximumTrackColor(slider, DefaultMaxTrackColor);
		}

		public static void MapThumbColor(ISliderHandler handler, ISlider slider)
		{

			handler.PlatformView?.UpdateThumbColor(slider, DefaultThumbColor);
		}

		[MissingMapper]
		public static void MapThumbImageSource(ISliderHandler handler, ISlider slider) { }

		void OnControlValueChanged(object? sender, EventArgs eventArgs)
		{
			if (PlatformView == null || VirtualView == null)
				return;

			VirtualView.Value = PlatformView.Value;
		}

		void OnDragStarted(object? sender, EventArgs e)
		{
			VirtualView?.DragStarted();
		}

		void OnDragStopped(object? sender, EventArgs e)
		{
			VirtualView?.DragCompleted();
		}
	}
}