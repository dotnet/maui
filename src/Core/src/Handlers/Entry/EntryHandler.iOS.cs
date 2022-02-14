using System;
using Foundation;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : ViewHandler<IEntry, MauiTextField>
	{
		protected override MauiTextField CreatePlatformView() =>
			new MauiTextField
			{
				BorderStyle = UITextBorderStyle.RoundedRect,
				ClipsToBounds = true
			};

		protected override void ConnectHandler(MauiTextField platformView)
		{
			platformView.ShouldReturn = OnShouldReturn;
			platformView.EditingChanged += OnEditingChanged;
			platformView.EditingDidBegin += OnEditingBegan;
			platformView.EditingDidEnd += OnEditingEnded;
			platformView.TextPropertySet += OnTextPropertySet;
			platformView.ShouldChangeCharacters += OnShouldChangeCharacters;
		}

		protected override void DisconnectHandler(MauiTextField platformView)
		{
			platformView.EditingChanged -= OnEditingChanged;
			platformView.EditingDidBegin -= OnEditingBegan;
			platformView.EditingDidEnd -= OnEditingEnded;
			platformView.TextPropertySet -= OnTextPropertySet;
			platformView.ShouldChangeCharacters -= OnShouldChangeCharacters;
		}

		public static void MapText(EntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateText(entry);

			// Any text update requires that we update any attributed string formatting
			MapFormatting(handler, entry);
		}

		public static void MapTextColor(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateTextColor(entry);

		public static void MapIsPassword(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsPassword(entry);

		public static void MapHorizontalTextAlignment(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateHorizontalTextAlignment(entry);

		public static void MapVerticalTextAlignment(EntryHandler handler, IEntry entry) =>
			handler?.PlatformView?.UpdateVerticalTextAlignment(entry);

		public static void MapIsTextPredictionEnabled(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsTextPredictionEnabled(entry);

		public static void MapMaxLength(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateMaxLength(entry);

		public static void MapPlaceholder(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdatePlaceholder(entry);

		public static void MapPlaceholderColor(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdatePlaceholder(entry);

		public static void MapIsReadOnly(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsReadOnly(entry);

		public static void MapKeyboard(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateKeyboard(entry);

		public static void MapReturnType(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateReturnType(entry);

		public static void MapFont(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateFont(entry, handler.GetRequiredService<IFontManager>());

		public static void MapCharacterSpacing(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateCharacterSpacing(entry);

		public static void MapCursorPosition(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateCursorPosition(entry);

		public static void MapSelectionLength(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateSelectionLength(entry);

		public static void MapClearButtonVisibility(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateClearButtonVisibility(entry);

		public static void MapFormatting(EntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateMaxLength(entry);

			// Update all of the attributed text formatting properties
			handler.PlatformView?.UpdateCharacterSpacing(entry);

			// Setting any of those may have removed text alignment settings,
			// so we need to make sure those are applied, too
			handler.PlatformView?.UpdateHorizontalTextAlignment(entry);
		}

		protected virtual bool OnShouldReturn(UITextField view)
		{
			view.ResignFirstResponder();

			// TODO: Focus next View

			VirtualView?.Completed();

			return false;
		}

		void OnEditingChanged(object? sender, EventArgs e) =>
			VirtualView.UpdateText(PlatformView.Text);

		void OnEditingBegan(object? sender, EventArgs e)
		{
			if (VirtualView == null || PlatformView == null)
				return;

			PlatformView?.UpdateSelectionLength(VirtualView);

			// TODO: Update IsFocused property
		}

		void OnEditingEnded(object? sender, EventArgs e)
		{
			if (VirtualView == null || PlatformView == null)
				return;

			VirtualView.UpdateText(PlatformView.Text);

			// TODO: Update IsFocused property
		}

		void OnTextPropertySet(object? sender, EventArgs e) =>
			VirtualView.UpdateText(PlatformView.Text);

		bool OnShouldChangeCharacters(UITextField textField, NSRange range, string replacementString) =>
			VirtualView.TextWithinMaxLength(textField.Text, range, replacementString);
	}
}