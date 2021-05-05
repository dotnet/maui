#nullable enable
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Handlers
{
	public partial class SliderHandler : ViewHandler<ISlider, Slider>
	{
		static Brush? DefaultForegroundColor;
		static Brush? DefaultBackgroundColor;
		
		PointerEventHandler? _pointerPressedHandler;
		PointerEventHandler? _pointerReleasedHandler;

		protected override Slider CreateNativeView()
		{
			var slider = new Slider
			{
				IsThumbToolTipEnabled = false
			};

			return slider;
		}

		protected override void ConnectHandler(Slider nativeView)
		{
			nativeView.ValueChanged += OnNativeValueChanged;

			_pointerPressedHandler = new PointerEventHandler(OnPointerPressed);
			_pointerReleasedHandler = new PointerEventHandler(OnPointerReleased);

			nativeView.AddHandler(UIElement.PointerPressedEvent, _pointerPressedHandler, true);
			nativeView.AddHandler(UIElement.PointerReleasedEvent, _pointerReleasedHandler, true);
			nativeView.AddHandler(UIElement.PointerCanceledEvent, _pointerReleasedHandler, true);
		}

		protected override void DisconnectHandler(Slider nativeView)
		{
			nativeView.ValueChanged -= OnNativeValueChanged;

			nativeView.RemoveHandler(UIElement.PointerPressedEvent, _pointerPressedHandler);
			nativeView.RemoveHandler(UIElement.PointerReleasedEvent, _pointerReleasedHandler);
			nativeView.RemoveHandler(UIElement.PointerCanceledEvent, _pointerReleasedHandler);

			_pointerPressedHandler = null;
			_pointerReleasedHandler = null;
		}

		protected override void SetupDefaults(Slider nativeView)
		{
			DefaultForegroundColor = nativeView.Foreground;
			DefaultBackgroundColor = nativeView.Background;
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

		[MissingMapper]
		public static void MapThumbColor(SliderHandler handler, ISlider slider) { }

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
	}
}