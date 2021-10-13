using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : ViewHandler<IEntry, NView>
	{

		protected override NView CreatePlatformView() => new NView()
		{
			BackgroundColor = Tizen.NUI.Color.Red
		};

		public static void MapBackground(IEntryHandler handler, IEntry entry)
		{
			handler.UpdateValue(nameof(handler.ContainerView));
			handler.ToPlatform()?.UpdateBackground(entry);
		}

		public static void MapText(IEntryHandler handler, IEntry entry)
		{
		}

		public static void MapTextColor(IEntryHandler handler, IEntry entry)
		{
		}

		public static void MapPlaceholder(IEntryHandler handler, IEntry entry)
		{
		}

		public static void MapPlaceholderColor(IEntryHandler handler, IEntry entry)
		{
		}

		public static void MapIsPassword(IEntryHandler handler, IEntry entry)
		{
		}

		public static void MapHorizontalTextAlignment(IEntryHandler handler, IEntry entry)
		{
		}

		public static void MapVerticalTextAlignment(IEntryHandler handler, IEntry entry)
		{
		}

		public static void MapMaxLength(IEntryHandler handler, IEntry entry)
		{
		}

		public static void MapIsReadOnly(IEntryHandler handler, IEntry entry)
		{
		}

		public static void MapIsTextPredictionEnabled(IEntryHandler handler, IEntry entry)
		{
		}

		public static void MapKeyboard(IEntryHandler handler, IEntry entry)
		{
		}

		public static void MapReturnType(IEntryHandler handler, IEntry entry)
		{
		}

		public static void MapFont(IEntryHandler handler, IEntry entry)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();
		}

		public static void MapClearButtonVisibility(IEntryHandler handler, IEntry entry)
		{
		}

		public static void MapFormatting(IEntryHandler handler, IEntry entry)
		{
		}

		public static void MapSelectionLength(IEntryHandler handler, IEntry entry)
		{
		}

		public static void MapCursorPosition(IEntryHandler handler, IEntry entry)
		{
		}

		public static void MapKeyboard(EditorHandler handler, IEntry entry)
		{
		}

		public static void MapSelectionLength(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateSelectionLength(entry);
		}

		public static void MapCursorPosition(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateSelectionLength(entry);
		}

		public static void MapKeyboard(EditorHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateKeyboard(entry);
		}

		[MissingMapper]
		public static void MapCharacterSpacing(IEntryHandler handler, IEntry entry) { }

	}
}