using System;
using Foundation;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : ViewHandler<IEntry, MauiTextField>
	{
		protected override MauiTextField CreateNativeView() =>
			new MauiTextField
			{
				BorderStyle = UITextBorderStyle.RoundedRect,
				ClipsToBounds = true
			};

		protected override void ConnectHandler(MauiTextField nativeView)
		{
			nativeView.ShouldReturn = OnShouldReturn;
			nativeView.EditingChanged += OnEditingChanged;
			nativeView.EditingDidBegin += OnEditingBegan;
			nativeView.EditingDidEnd += OnEditingEnded;
			nativeView.TextPropertySet += OnTextPropertySet;
			nativeView.ShouldChangeCharacters += OnShouldChangeCharacters;
		}

		protected override void DisconnectHandler(MauiTextField nativeView)
		{
			nativeView.EditingChanged -= OnEditingChanged;
			nativeView.EditingDidBegin -= OnEditingBegan;
			nativeView.EditingDidEnd -= OnEditingEnded;
			nativeView.TextPropertySet -= OnTextPropertySet;
			nativeView.ShouldChangeCharacters -= OnShouldChangeCharacters;
		}

		public static void MapText(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateText(entry);

			// Any text update requires that we update any attributed string formatting
			MapFormatting(handler, entry);
		}

		public static void MapTextColor(EntryHandler handler, IEntry entry) =>
			handler.NativeView?.UpdateTextColor(entry);

		public static void MapIsPassword(EntryHandler handler, IEntry entry) =>
			handler.NativeView?.UpdateIsPassword(entry);

		public static void MapHorizontalTextAlignment(EntryHandler handler, IEntry entry) =>
			handler.NativeView?.UpdateHorizontalTextAlignment(entry);

		public static void MapVerticalTextAlignment(EntryHandler handler, IEntry entry) =>
			handler?.NativeView?.UpdateVerticalTextAlignment(entry);

		public static void MapIsTextPredictionEnabled(EntryHandler handler, IEntry entry) =>
			handler.NativeView?.UpdateIsTextPredictionEnabled(entry);

		public static void MapMaxLength(EntryHandler handler, IEntry entry) =>
			handler.NativeView?.UpdateMaxLength(entry);

		public static void MapPlaceholder(EntryHandler handler, IEntry entry) =>
			handler.NativeView?.UpdatePlaceholder(entry);

		public static void MapPlaceholderColor(EntryHandler handler, IEntry entry) =>
			handler.NativeView?.UpdatePlaceholder(entry);

		public static void MapIsReadOnly(EntryHandler handler, IEntry entry) =>
			handler.NativeView?.UpdateIsReadOnly(entry);

		public static void MapKeyboard(EntryHandler handler, IEntry entry) =>
			handler.NativeView?.UpdateKeyboard(entry);

		public static void MapReturnType(EntryHandler handler, IEntry entry) =>
			handler.NativeView?.UpdateReturnType(entry);

		public static void MapFont(EntryHandler handler, IEntry entry) =>
			handler.NativeView?.UpdateFont(entry, handler.GetRequiredService<IFontManager>());

		public static void MapCharacterSpacing(EntryHandler handler, IEntry entry) =>
			handler.NativeView?.UpdateCharacterSpacing(entry);

		public static void MapCursorPosition(EntryHandler handler, IEntry entry) =>
			handler.NativeView?.UpdateCursorPosition(entry);

		public static void MapSelectionLength(EntryHandler handler, IEntry entry) =>
			handler.NativeView?.UpdateSelectionLength(entry);

		public static void MapClearButtonVisibility(EntryHandler handler, IEntry entry) =>
			handler.NativeView?.UpdateClearButtonVisibility(entry);

		public static void MapFormatting(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateMaxLength(entry);

			// Update all of the attributed text formatting properties
			handler.NativeView?.UpdateCharacterSpacing(entry);

			// Setting any of those may have removed text alignment settings,
			// so we need to make sure those are applied, too
			handler.NativeView?.UpdateHorizontalTextAlignment(entry);
		}

		protected virtual bool OnShouldReturn(UITextField view)
		{
			view.ResignFirstResponder();

			// TODO: Focus next View

			VirtualView?.Completed();

			return false;
		}

		void OnEditingChanged(object? sender, EventArgs e) =>
			VirtualView.UpdateText(NativeView.Text);

		void OnEditingBegan(object? sender, EventArgs e)
		{
			if (VirtualView == null || NativeView == null)
				return;

			NativeView?.UpdateSelectionLength(VirtualView);

			// TODO: Update IsFocused property
		}

		void OnEditingEnded(object? sender, EventArgs e)
		{
			if (VirtualView == null || NativeView == null)
				return;

			VirtualView.UpdateText(NativeView.Text);

			// TODO: Update IsFocused property
		}

		void OnTextPropertySet(object? sender, EventArgs e) =>
			VirtualView.UpdateText(NativeView.Text);

		bool OnShouldChangeCharacters(UITextField textField, NSRange range, string replacementString) =>
			VirtualView.TextWithinMaxLength(textField.Text, range, replacementString);
	}
}