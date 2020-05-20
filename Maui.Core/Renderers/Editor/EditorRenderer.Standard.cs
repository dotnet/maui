namespace System.Maui.Platform
{
	public partial class EditorRenderer : AbstractViewRenderer<IEditor, object>
	{
		protected override object CreateView() => throw new NotImplementedException();

		public static void MapPropertyColor(IViewRenderer renderer, IEditor editor) { }
		public static void MapPropertyPlaceholder(IViewRenderer renderer, IEditor editor) { }
		public static void MapPropertyPlaceholderColor(IViewRenderer renderer, IEditor editor) { }
		public static void MapPropertyText(IViewRenderer renderer, IEditor editor) { }
		public static void MapPropertyMaxLenght(IViewRenderer renderer, IEditor editor) { }
		public static void MapPropertyAutoSize(IViewRenderer renderer, IEditor editor) { }
	}
}