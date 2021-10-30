#nullable enable
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Handlers
{
	public partial class SliderHandler : ViewHandler<ISlider, MauiSlider>
	{
		static Brush? DefaultForegroundColor;
		static Brush? DefaultBackgroundColor;
		static Brush? DefaultThumbColor;

		PointerEventHandler? _pointerPressedHandler;
		PointerEventHandler? _pointerReleasedHandler;

		protected override MauiSlider CreateNativeView()
		{
			var slider = new MauiSlider
			{
				IsThumbToolTipEnabled = false
			};

			return slider;
		}

		protected override void ConnectHandler(MauiSlider nativeView)
		{
			SetupDefaults(NativeView);

			nativeView.ValueChanged += OnNativeValueChanged;
			nativeView.Ready += OnNativeViewReady;

			_pointerPressedHandler = new PointerEventHandler(OnPointerPressed);
			_pointerReleasedHandler = new PointerEventHandler(OnPointerReleased);

			nativeView.AddHandler(UIElement.PointerPressedEvent, _pointerPressedHandler, true);
			nativeView.AddHandler(UIElement.PointerReleasedEvent, _pointerReleasedHandler, true);
			nativeView.AddHandler(UIElement.PointerCanceledEvent, _pointerReleasedHandler, true);
		}

		protected override void DisconnectHandler(MauiSlider nativeView)
		{
			nativeView.ValueChanged -= OnNativeValueChanged;
			nativeView.Ready -= OnNativeViewReady;

			nativeView.RemoveHandler(UIElement.PointerPressedEvent, _pointerPressedHandler);
			nativeView.RemoveHandler(UIElement.PointerReleasedEvent, _pointerReleasedHandler);
			nativeView.RemoveHandler(UIElement.PointerCanceledEvent, _pointerReleasedHandler);

			_pointerPressedHandler = null;
			_pointerReleasedHandler = null;
		}

		void SetupDefaults(MauiSlider nativeView)
		{
			DefaultForegroundColor = nativeView.Foreground;
			DefaultBackgroundColor = nativeView.Background;
			DefaultThumbColor = nativeView.Thumb?.Background;
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
			handler.NativeView?.UpdateMinimumTrackColor(slider, DefaultForegroundColor);
		}

		public static void MapMaximumTrackColor(SliderHandler handler, ISlider slider)
		{
			handler.NativeView?.UpdateMaximumTrackColor(slider, DefaultBackgroundColor);
		}

		public static void MapThumbColor(SliderHandler handler, ISlider slider)
		{
			handler.NativeView?.UpdateThumbColor(slider, DefaultThumbColor);
		}

		public static void MapThumbImageSource(SliderHandler handler, ISlider slider)
		{
			var provider = handler.GetRequiredService<IImageSourceServiceProvider>();

			handler.NativeView?.UpdateThumbImageSourceAsync(slider, provider)
 				.FireAndForget(handler);
		}

		void OnNativeValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
		{
			if (VirtualView != null)
				VirtualView.Value = e.NewValue;
		}

		void OnPointerPressed(object? sender, PointerRoutedEventArgs e)
		{
			VirtualView?.DragStarted();
		}

		void OnPointerReleased(object? sender, PointerRoutedEventArgs e)
		{
			VirtualView?.DragCompleted();
		}

		void OnNativeViewReady(object? sender, EventArgs e)
		{
			if (VirtualView != null)
				NativeView?.UpdateThumbColor(VirtualView, DefaultThumbColor);
		}
	}
}