using System;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : WidgetHandler<IEntry, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapText(IFrameworkElementHandler handler, IEntry entry) { }
		public static void MapTextColor(IFrameworkElementHandler handler, IEntry entry) { }
		public static void MapIsPassword(IFrameworkElementHandler handler, IEntry entry) { }
		public static void MapHorizontalTextAlignment(IFrameworkElementHandler handler, IEntry entry) { }
		public static void MapIsTextPredictionEnabled(IFrameworkElementHandler handler, IEntry entry) { }
		public static void MapMaxLength(IFrameworkElementHandler handler, IEntry entry) { }
		public static void MapPlaceholder(IFrameworkElementHandler handler, IEntry entry) { }
		public static void MapIsReadOnly(IFrameworkElementHandler handler, IEntry entry) { }
		public static void MapKeyboard(IFrameworkElementHandler handler, IEntry entry) { }
		public static void MapFont(IFrameworkElementHandler handler, IEntry entry) { }
		public static void MapReturnType(IFrameworkElementHandler handler, IEntry entry) { }
		public static void MapClearButtonVisibility(IFrameworkElementHandler handler, IEntry entry) { }
		public static void MapCharacterSpacing(IFrameworkElementHandler handler, IEntry entry) { }
	}
}