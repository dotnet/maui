using Gtk;

namespace Microsoft.Maui.Handlers
{
	public partial class SliderHandler : ViewHandler<ISlider, Scale>
	{
		protected override Scale CreateNativeView()
		{
			var adjustment = new Adjustment(0, 0, 1, 1, 1, 1);
			return new Scale(Orientation.Horizontal, adjustment);
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
