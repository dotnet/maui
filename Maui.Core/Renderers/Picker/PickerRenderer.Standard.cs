namespace System.Maui.Platform
{
	public partial class PickerRenderer : AbstractViewRenderer<IPicker, object>
	{
		protected override object CreateView() => throw new NotImplementedException();

		public static void MapPropertyTitle(IViewRenderer renderer, IPicker picker) { }
		public static void MapPropertyTitleColor(IViewRenderer renderer, IPicker picker) { }
		public static void MapPropertyTextColor(IViewRenderer renderer, IPicker picker) { }
		public static void MapPropertySelectedIndex(IViewRenderer renderer, IPicker picker) { }
	}
}
