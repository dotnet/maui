using System;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : ViewHandler<IEntry, TextBox>
	{
		protected override TextBox CreateNativeView() =>  new MauiTextBox { Style = Microsoft.UI.Xaml.Application.Current.Resources["MauiTextBoxStyle"] as Microsoft.UI.Xaml.Style };

		[MissingMapper]
		public static void MapText(IViewHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapTextColor(IViewHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapIsPassword(IViewHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapHorizontalTextAlignment(IViewHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapIsTextPredictionEnabled(IViewHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapMaxLength(IViewHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapPlaceholder(IViewHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapIsReadOnly(IViewHandler handler, IEntry entry) { }

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