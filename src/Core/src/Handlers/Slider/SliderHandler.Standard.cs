using System;

namespace Microsoft.Maui.Handlers
{
	public partial class SliderHandler : WidgetHandler<ISlider, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapMinimum(IFrameworkElementHandler handler, ISlider slider) { }
		public static void MapMaximum(IFrameworkElementHandler handler, ISlider slider) { }
		public static void MapValue(IFrameworkElementHandler handler, ISlider slider) { }
		public static void MapMinimumTrackColor(IFrameworkElementHandler handler, ISlider slider) { }
		public static void MapMaximumTrackColor(IFrameworkElementHandler handler, ISlider slider) { }
		public static void MapThumbColor(IFrameworkElementHandler handler, ISlider slider) { }
	}
}