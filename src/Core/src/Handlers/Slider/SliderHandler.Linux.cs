using System;
using Gtk;

namespace Microsoft.Maui.Handlers
{
	public partial class SliderHandler : ViewHandler<ISlider, Scale>
	{
		protected override Scale CreateNativeView()
		{
			return new Scale(Orientation.Horizontal,0,1,.1);
		}

		protected override void ConnectHandler(Scale nativeView)
		{
			base.ConnectHandler(nativeView);
			
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");

			nativeView.ValueChanged += OnNativeViewValueChanged;
		}

		protected override void DisconnectHandler(Scale nativeView)
		{
			base.DisconnectHandler(nativeView);
			
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");

			nativeView.ValueChanged -= OnNativeViewValueChanged;

		}

		void OnNativeViewValueChanged(object? sender, EventArgs e)
		{
			if (sender is not Scale nativeView || VirtualView is not {} virtualView) 
				return;
			
			virtualView.Value = nativeView.Value;
			
		}

		public static void MapMinimum(SliderHandler handler, ISlider slider)
		{
			handler.NativeView?.UpdateRange(slider);
		}

		public static void MapMaximum(SliderHandler handler, ISlider slider)
		{
			handler.NativeView?.UpdateRange(slider);
		}

		public static void MapValue(SliderHandler handler, ISlider slider)
		{
			handler.NativeView?.UpdateValue(slider);
		}

		[MissingMapper]
		public static void MapMinimumTrackColor(SliderHandler handler, ISlider slider) { }

		[MissingMapper]
		public static void MapMaximumTrackColor(SliderHandler handler, ISlider slider) { }

		[MissingMapper]
		public static void MapThumbColor(SliderHandler handler, ISlider slider) { }
	}
}
