namespace System.Maui.Platform
{
	public partial class SliderRenderer : AbstractViewRenderer<ISlider, object>
	{
		protected override object CreateView() => throw new NotImplementedException();

		public static void MapPropertyMinimum(IViewRenderer renderer, ISlider slider) { }
		public static void MapPropertyMaximum(IViewRenderer renderer, ISlider slider) { }
		public static void MapPropertyValue(IViewRenderer renderer, ISlider slider) { }
		public static void MapPropertyMinimumTrackColor(IViewRenderer renderer, ISlider slider) { }
		public static void MapPropertyMaximumTrackColor(IViewRenderer renderer, ISlider slider) { }
		public static void MapPropertyThumbColor(IViewRenderer renderer, ISlider slider) { }
	}
}