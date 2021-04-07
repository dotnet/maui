using System;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class SliderHandler : ViewHandler<ISlider, Slider>
	{
		protected override Slider CreateNativeView() => new Slider();

		[MissingMapper]
		public static void MapMinimum(IViewHandler handler, ISlider slider) { }

		[MissingMapper]
		public static void MapMaximum(IViewHandler handler, ISlider slider) { }

		[MissingMapper]
		public static void MapValue(IViewHandler handler, ISlider slider) { }

		[MissingMapper]
		public static void MapMinimumTrackColor(IViewHandler handler, ISlider slider) { }

		[MissingMapper]
		public static void MapMaximumTrackColor(IViewHandler handler, ISlider slider) { }

		[MissingMapper]
		public static void MapThumbColor(IViewHandler handler, ISlider slider) { }
	}
}