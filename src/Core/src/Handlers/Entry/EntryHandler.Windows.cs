namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : ViewHandler<IEntry, MauiTextBox>
	{
		protected override MauiTextBox CreateNativeView() =>  
			new MauiTextBox { Style = UI.Xaml.Application.Current.Resources["MauiTextBoxStyle"] as UI.Xaml.Style };

		public static void MapText(EntryHandler handler, IEntry entry) 
		{
			handler.NativeView?.UpdateText(entry);
		}

		[MissingMapper]
		public static void MapTextColor(IViewHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapIsPassword(IViewHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapHorizontalTextAlignment(IViewHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapIsTextPredictionEnabled(IViewHandler handler, IEntry entry) { }

		public static void MapMaxLength(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateMaxLength(entry);
		}

		[MissingMapper]
		public static void MapPlaceholder(IViewHandler handler, IEntry entry) { }

		public static void MapIsReadOnly(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateIsReadOnly(entry);
		}

		[MissingMapper]
		public static void MapFont(IViewHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapReturnType(IViewHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapClearButtonVisibility(IViewHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapCharacterSpacing(IViewHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapKeyboard(IViewHandler handler, IEntry entry) { }
	}
}