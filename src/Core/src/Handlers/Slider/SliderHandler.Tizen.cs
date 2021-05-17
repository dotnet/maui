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

		protected override ESlider CreateNativeView() => new ESlider(NativeParent);

		protected override void ConnectHandler(ESlider nativeView)
		{
			nativeView!.ValueChanged += OnControlValueChanged;
			nativeView!.DragStarted += OnDragStarted;
			nativeView!.DragStopped += OnDragStopped;
		}

		protected override void DisconnectHandler(ESlider nativeView)
		{
			nativeView!.ValueChanged -= OnControlValueChanged;
			nativeView!.DragStarted -= OnDragStarted;
			nativeView!.DragStopped -= OnDragStopped;
		}

		protected override void SetupDefaults(ESlider nativeView)
		{
			DefaultMinTrackColor = nativeView.GetBarColor();
			DefaultMaxTrackColor = nativeView.GetBackgroundColor();
			DefaultThumbColor = nativeView.GetHandlerColor();
		}

		public static void MapMinimum(SliderHandler handler, ISlider slider)
		{
			handler.NativeView?.UpdateMinimum(slider);
		}

		public static void MapMaximum(SliderHandler handler, ISlider slider)
		{
			handler.NativeView?.UpdateMaximum(slider);
		}


		public static void MapValue(SliderHandler handler, ISlider slider)
		{
			handler.NativeView?.UpdateValue(slider);
		}

		public static void MapMinimumTrackColor(SliderHandler handler, ISlider slider)
		{
			handler.NativeView?.UpdateMinimumTrackColor(slider, DefaultMinTrackColor);
		}

		public static void MapMaximumTrackColor(SliderHandler handler, ISlider slider)
		{
			handler.NativeView?.UpdateMaximumTrackColor(slider, DefaultMaxTrackColor);
		}

		public static void MapThumbColor(SliderHandler handler, ISlider slider)
		{

			handler.NativeView?.UpdateThumbColor(slider, DefaultThumbColor);
		}

		void OnControlValueChanged(object? sender, EventArgs eventArgs)
		{
			if (NativeView == null || VirtualView == null)
				return;

			VirtualView.Value = NativeView.Value;
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