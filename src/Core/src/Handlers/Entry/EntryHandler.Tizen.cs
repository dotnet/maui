using System;
using Tizen.UIExtensions.ElmSharp;
using EEntry = ElmSharp.Entry;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : ViewHandler<IEntry, Entry>
	{

		protected override Entry CreatePlatformView()
		{
			_ = NativeParent ?? throw new ArgumentNullException(nameof(NativeParent));

			return new EditfieldEntry(NativeParent)
			{
				IsSingleLine = true
			};
		}

		protected override void ConnectHandler(Entry platformView)
		{
			platformView.Activated += OnCompleted;
			platformView.CursorPositionChanged += OnCursorChanged;

			// In order to know when the selection is cleared, SelectionCleared event has been used.
			// Because CursorChanged event is still invoked with the selected text when an user clears selection. It is an known issue in EFL.
			platformView.SelectionCleared += OnSelectionCleared;

			platformView.TextChanged += OnTextChanged;
			platformView.EntryLayoutFocused += OnFocused;
			platformView.EntryLayoutUnfocused += OnUnfocused;
			platformView.PrependMarkUpFilter(MaxLengthFilter);


			// TODO: Fix me later
			// An initial CursorPosition is set after layouting to avoid timing issue when the EditField entry is initialized.
			//if (VirtualView != null)
			//{
			//	MainThread.BeginInvokeOnMainThread(() =>
			//	{
			//		platformView.UpdateSelectionLength(VirtualView);
			//	});
			//}
		}

		protected override void DisconnectHandler(Entry platformView)
		{
			platformView.Activated -= OnCompleted;
			platformView.CursorPositionChanged -= OnCursorChanged;
			platformView.TextChanged -= OnTextChanged;
			platformView.EntryLayoutFocused -= OnFocused;
			platformView.EntryLayoutUnfocused -= OnUnfocused;
		}

		public static void MapBackground(IEntryHandler handler, IEntry entry)
		{
			handler.UpdateValue(nameof(handler.ContainerView));
			handler.ToPlatform()?.UpdateBackground(entry);
		}

		public static void MapText(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateText(entry);

			// Any text update requires that we update any attributed string formatting
			MapFormatting(handler, entry);
		}

		public static void MapTextColor(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateTextColor(entry);
		}

		public static void MapPlaceholder(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdatePlaceholder(entry);
		}

		public static void MapPlaceholderColor(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdatePlaceholderColor(entry);
		}

		public static void MapIsPassword(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateIsPassword(entry);
		}

		public static void MapHorizontalTextAlignment(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateHorizontalTextAlignment(entry);
		}

		public static void MapVerticalTextAlignment(IEntryHandler handler, IEntry entry)
		{
			handler?.PlatformView?.UpdateVerticalTextAlignment(entry);
		}

		public static void MapMaxLength(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateMaxLength(entry);
		}

		public static void MapIsReadOnly(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateIsReadOnly(entry);
		}

		public static void MapIsTextPredictionEnabled(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateIsTextPredictionEnabled(entry);
		}

		public static void MapKeyboard(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateKeyboard(entry);
		}

		public static void MapReturnType(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateReturnType(entry);
		}

		public static void MapFont(IEntryHandler handler, IEntry entry)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(entry, fontManager);
		}

		public static void MapClearButtonVisibility(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateClearButtonVisibility(entry);
		}

		public static void MapFormatting(IEntryHandler handler, IEntry entry)
		{
			// Update all of the attributed text formatting properties
			// Setting any of those may have removed text alignment settings,
			// so we need to make sure those are applied, too
			handler.PlatformView?.UpdateMaxLength(entry);
			handler.PlatformView?.UpdateHorizontalTextAlignment(entry);
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

		string? MaxLengthFilter(EEntry entry, string s)
		{
			if (VirtualView == null || PlatformView == null)
				return null;

			if (entry.Text.Length < VirtualView.MaxLength)
				return s;

			return null;
		}

		void OnTextChanged(object? sender, EventArgs e)
		{
			if (VirtualView == null || PlatformView == null)
				return;

			VirtualView.Text = PlatformView.Text;
		}

		void OnCursorChanged(object? sender, EventArgs e)
		{
			if (VirtualView == null || PlatformView == null)
				return;

			var position = PlatformView.CursorPosition;

			PlatformView.GetSelectRegion(out int start, out int end);

			if (start > -1)
			{
				position = (start < end) ? start : end;
				var selectionLength = Math.Abs(end - start);
				VirtualView.SelectionLength = selectionLength;
			}

			VirtualView.CursorPosition = position;
		}

		void OnSelectionCleared(object? sender, EventArgs e)
		{
			if (VirtualView == null || PlatformView == null)
				return;

			if (PlatformView.IsFocused)
			{
				VirtualView.SelectionLength = 0;
				VirtualView.CursorPosition = PlatformView.CursorPosition;
			}
		}

		void OnCompleted(object? sender, EventArgs e)
		{
			if (PlatformView == null)
				return;

			PlatformView.SetFocus(false);
		}
	}
}