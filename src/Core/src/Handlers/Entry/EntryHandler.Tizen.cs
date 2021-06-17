using System;
using Tizen.UIExtensions.ElmSharp;
using SmartEvent = ElmSharp.SmartEvent;
using EEntry = ElmSharp.Entry;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : ViewHandler<IEntry, Entry>
	{

		protected override Entry CreateNativeView()
		{
			return new EditfieldEntry(NativeParent)
			{
				IsSingleLine = true
			};
		}

		protected override void ConnectHandler(Entry nativeView)
		{
			nativeView.Activated += OnCompleted;
			nativeView.CursorChanged += OnCursorChanged;

			// In order to know when the selection is cleared, "selecton,cleared" event has been used.
			// Because CursorChanged event is still invoked with the selected text when an user clears selection. It is an known issue in EFL.
			SmartEvent selectionCleared = new SmartEvent(nativeView, nativeView.RealHandle, ThemeConstants.Entry.Signals.SelectionCleared);
			selectionCleared.On += OnSelectionCleared;

			nativeView.TextChanged += OnTextChanged;
			nativeView.EntryLayoutFocused += OnFocused;
			nativeView.EntryLayoutUnfocused += OnUnfocused;

			nativeView.PrependMarkUpFilter(MaxLengthFilter);

			// TODO: Fix me later
			// An initial CursorPosition is set after layouting to avoid timing issue when the EditField entry is initialized.
			//if (VirtualView != null)
			//{
			//	MainThread.BeginInvokeOnMainThread(() =>
			//	{
			//		nativeView.UpdateSelectionLength(VirtualView);
			//	});
			//}
		}

		protected override void DisconnectHandler(Entry nativeView)
		{
			nativeView.Activated -= OnCompleted;
			nativeView.CursorChanged -= OnCursorChanged;
			nativeView.TextChanged -= OnTextChanged;
			nativeView.EntryLayoutFocused -= OnFocused;
			nativeView.EntryLayoutUnfocused -= OnUnfocused;
		}

		public static void MapText(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateText(entry);

			// Any text update requires that we update any attributed string formatting
			MapFormatting(handler, entry);
		}

		public static void MapTextColor(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateTextColor(entry);
		}

		public static void MapPlaceholder(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdatePlaceholder(entry);
		}

		public static void MapIsPassword(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateIsPassword(entry);
		}

		public static void MapHorizontalTextAlignment(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateHorizontalTextAlignment(entry);
		}

		public static void MapMaxLength(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateMaxLength(entry);
		}

		public static void MapIsReadOnly(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateIsReadOnly(entry);
		}

		public static void MapIsTextPredictionEnabled(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateIsTextPredictionEnabled(entry);
		}

		public static void MapKeyboard(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateKeyboard(entry);
		}

		public static void MapReturnType(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateReturnType(entry);
		}

		public static void MapFont(EntryHandler handler, IEntry entry)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.NativeView?.UpdateFont(entry, fontManager);
		}

		public static void MapClearButtonVisibility(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateClearButtonVisibility(entry);
		}

		public static void MapFormatting(EntryHandler handler, IEntry entry)
		{
			// Update all of the attributed text formatting properties
			// Setting any of those may have removed text alignment settings,
			// so we need to make sure those are applied, too
			handler.NativeView?.UpdateMaxLength(entry);
			handler.NativeView?.UpdateHorizontalTextAlignment(entry);
		}

		public static void MapSelectionLength(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateSelectionLength(entry);
		}

		public static void MapCursorPosition(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateSelectionLength(entry);
		}

		public static void MapKeyboard(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdateKeyboard(editor);
		}

		[MissingMapper]
		public static void MapCharacterSpacing(EntryHandler handler, IEntry entry) { }

		string? MaxLengthFilter(EEntry entry, string s)
		{
			if (VirtualView == null || NativeView == null)
				return null;

			if (entry.Text.Length < VirtualView.MaxLength)
				return s;

			return null;
		}

		void OnTextChanged(object? sender, EventArgs e)
		{
			if (VirtualView == null || NativeView == null)
				return;

			VirtualView.Text = NativeView.Text;
		}

		void OnCursorChanged(object? sender, EventArgs e)
		{
			if (VirtualView == null || NativeView == null)
				return;

			var position = NativeView.CursorPosition;

			NativeView.GetSelectRegion(out int start, out int end);

			if (start > -1)
			{
				position = (start < end) ? start : end;
				var selectionLength = Math.Abs(end - start);
				VirtualView.SelectionLength = selectionLength;
			}

			VirtualView.CursorPosition = position;
		}

		void OnSelectionCleared(object sender, EventArgs e)
		{
			if (VirtualView == null || NativeView == null)
				return;

			if (NativeView.IsFocused)
			{
				VirtualView.SelectionLength = 0;
				VirtualView.CursorPosition = NativeView.CursorPosition;
			}
		}

		void OnCompleted(object? sender, EventArgs e)
		{
			if (NativeView == null)
				return;

			NativeView.SetFocus(false);
		}
	}
}