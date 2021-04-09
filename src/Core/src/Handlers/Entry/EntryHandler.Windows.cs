using System;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Platform.Windows;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : ViewHandler<IEntry, TextBox>
	{
		protected override TextBox CreateNativeView() => new TextBox();

		public static void MapText(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateText(entry.Text, entry.TextTransform);
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
		public static void MapCharacterSpacing(IViewHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapKeyboard(IViewHandler handler, IEntry entry) { } 
	}
}