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
		static Brush? DefaultMinimumTrackColor;
		static Brush? DefaultMaximumTrackColor;
		static Brush? DefaultThumbColor;

		PointerEventHandler? _pointerPressedHandler;
		PointerEventHandler? _pointerReleasedHandler;

		protected override MauiSlider CreatePlatformView()
		{
			var slider = new MauiSlider
			{
				IsThumbToolTipEnabled = false
			};

			return slider;
		}

		protected override void ConnectHandler(MauiSlider platformView)
		{
			SetupDefaults(PlatformView);

			platformView.ValueChanged += OnPlatformValueChanged;
			platformView.Ready += OnPlatformViewReady;

			_pointerPressedHandler = new PointerEventHandler(OnPointerPressed);
			_pointerReleasedHandler = new PointerEventHandler(OnPointerReleased);

			platformView.AddHandler(UIElement.PointerPressedEvent, _pointerPressedHandler, true);
			platformView.AddHandler(UIElement.PointerReleasedEvent, _pointerReleasedHandler, true);
			platformView.AddHandler(UIElement.PointerCanceledEvent, _pointerReleasedHandler, true);
		}

		protected override void DisconnectHandler(MauiSlider platformView)
		{
			platformView.ValueChanged -= OnPlatformValueChanged;
			platformView.Ready -= OnPlatformViewReady;

			platformView.RemoveHandler(UIElement.PointerPressedEvent, _pointerPressedHandler);
			platformView.RemoveHandler(UIElement.PointerReleasedEvent, _pointerReleasedHandler);
			platformView.RemoveHandler(UIElement.PointerCanceledEvent, _pointerReleasedHandler);

			_pointerPressedHandler = null;
			_pointerReleasedHandler = null;
		}

		void SetupDefaults(MauiSlider platformView)
		{
			DefaultMinimumTrackColor = platformView.Resources["SliderTrackValueFill"] as Brush;
			DefaultMaximumTrackColor = platformView.Resources["SliderTrackFill"] as Brush;
			DefaultThumbColor = platformView.Thumb?.Background;
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
			handler.PlatformView?.UpdateMinimumTrackColor(slider, DefaultMinimumTrackColor);
		}

		public static void MapMaximumTrackColor(ISliderHandler handler, ISlider slider)
		{
			handler.PlatformView?.UpdateMaximumTrackColor(slider, DefaultMaximumTrackColor);
		}

		public static void MapThumbColor(ISliderHandler handler, ISlider slider)
		{
			handler.PlatformView?.UpdateThumbColor(slider, DefaultThumbColor);
		}

		public static void MapThumbImageSource(ISliderHandler handler, ISlider slider)
		{
			var provider = handler.GetRequiredService<IImageSourceServiceProvider>();

			handler.PlatformView?.UpdateThumbImageSourceAsync(slider, provider)
 				.FireAndForget(handler);
		}

		void OnPlatformValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
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

		void OnPlatformViewReady(object? sender, EventArgs e)
		{
			if (VirtualView != null)
				PlatformView?.UpdateThumbColor(VirtualView, DefaultThumbColor);
		}
	}
}