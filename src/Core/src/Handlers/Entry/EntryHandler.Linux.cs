using Gtk;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : ViewHandler<IEditor, Entry>
	{
		protected override Entry CreateNativeView()
		{
			return new Entry();
		}

		public static void MapText(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateText(entry);
		}

		[MissingMapper]
		public static void MapTextColor(EntryHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapIsPassword(EntryHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapHorizontalTextAlignment(EntryHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapIsTextPredictionEnabled(EntryHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapMaxLength(EntryHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapPlaceholder(EntryHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapIsReadOnly(EntryHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapFont(EntryHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapReturnType(EntryHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapClearButtonVisibility(EntryHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapCharacterSpacing(EntryHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapKeyboard(EntryHandler handler, IEntry entry) { }
	}
}