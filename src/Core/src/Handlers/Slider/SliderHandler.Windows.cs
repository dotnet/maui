using System;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class SliderHandler : AbstractViewHandler<ISlider, Slider>
	{
		protected override Slider CreateNativeView() => new Slider();

		public static void MapMinimum(IViewHandler handler, ISlider slider) { }
		public static void MapMaximum(IViewHandler handler, ISlider slider) { }
		public static void MapValue(IViewHandler handler, ISlider slider) { }
		public static void MapMinimumTrackColor(IViewHandler handler, ISlider slider) { }
		public static void MapMaximumTrackColor(IViewHandler handler, ISlider slider) { }
		public static void MapThumbColor(IViewHandler handler, ISlider slider) { }
	}
}