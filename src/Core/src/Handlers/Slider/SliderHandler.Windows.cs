#nullable enable
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;

namespace Microsoft.Maui.Handlers
{
	public partial class SliderHandler : ViewHandler<ISlider, Slider>
	{
		Size? _thumbSize;
		PointerEventHandler? _pointerPressedHandler;
		PointerEventHandler? _pointerReleasedHandler;

		protected override Slider CreatePlatformView()
		{
			// MauiSlider is an internal type
			var slider = new MauiSlider
			{
				IsThumbToolTipEnabled = false
			};

			return slider;
		}

		protected override void ConnectHandler(Slider platformView)
		{
			platformView.Loaded += OnPlatformViewLoaded;

			_pointerPressedHandler = new PointerEventHandler(OnPointerPressed);
			_pointerReleasedHandler = new PointerEventHandler(OnPointerReleased);

			platformView.AddHandler(UIElement.PointerPressedEvent, _pointerPressedHandler, true);
			platformView.AddHandler(UIElement.PointerReleasedEvent, _pointerReleasedHandler, true);
			platformView.AddHandler(UIElement.PointerCanceledEvent, _pointerReleasedHandler, true);
		}

		protected override void DisconnectHandler(Slider platformView)
		{
			platformView.Loaded -= OnPlatformViewLoaded;
			platformView.ValueChanged -= OnPlatformValueChanged;

			platformView.RemoveHandler(UIElement.PointerPressedEvent, _pointerPressedHandler);
			platformView.RemoveHandler(UIElement.PointerReleasedEvent, _pointerReleasedHandler);
			platformView.RemoveHandler(UIElement.PointerCanceledEvent, _pointerReleasedHandler);

			_pointerPressedHandler = null;
			_pointerReleasedHandler = null;

			_thumbSize = null;
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

			if (handler?.PlatformView is MauiSlider mauiSlider)
			{
				mauiSlider.UpdateThumbImageSourceAsync(slider, provider, (handler as SliderHandler)?._thumbSize).FireAndForget(handler);
			}
		}

		internal static void MapBackgroundColor(ISliderHandler handler, ISlider slider)
		{
			if (handler.PlatformView is MauiSlider mauiSlider)
			{
				mauiSlider.UpdateBackgroundColor(slider);
			}
		}

		void OnPlatformViewLoaded(object sender, RoutedEventArgs e)
		{
			var platformView = sender as Slider;

			if (platformView is not null)
			{
				var thumb = platformView.GetFirstDescendant<Thumb>();

				if (thumb is not null)
					_thumbSize = new Size(thumb.Width, thumb.Height);

				platformView.ValueChanged += OnPlatformValueChanged;
			}
		}

		void OnPlatformValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
		{
			if (VirtualView != null && VirtualView.Value != e.NewValue)
			{
				VirtualView.Value = e.NewValue;
			}
		}

		void OnPointerPressed(object? sender, PointerRoutedEventArgs e)
		{
			VirtualView?.DragStarted();
		}

		void OnPointerReleased(object? sender, PointerRoutedEventArgs e)
		{
			VirtualView?.DragCompleted();
		}
	}
}