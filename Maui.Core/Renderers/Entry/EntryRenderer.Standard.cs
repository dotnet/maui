
namespace System.Maui.Platform
{
	public partial class EntryRenderer : AbstractViewRenderer<ITextInput, object>
	{
		protected override object CreateView() => throw new NotImplementedException();

		public static void MapPropertyColor(IViewRenderer renderer, ITextInput entry) { }
		public static void MapPropertyPlaceholder(IViewRenderer renderer, ITextInput entry) { }
		public static void MapPropertyPlaceholderColor(IViewRenderer renderer, ITextInput entry) { }
		public static void MapPropertyText(IViewRenderer renderer, ITextInput entry) { }
	}
}
