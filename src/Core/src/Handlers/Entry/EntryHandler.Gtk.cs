using System;
using Gtk;

namespace Microsoft.Maui.Handlers
{

	// https://developer.gnome.org/gtk3/stable/GtkEntry.html

	public partial class EntryHandler : ViewHandler<IEntry, Entry>
	{

		protected override Entry CreateNativeView()
		{
			return new();
		}

		protected override void ConnectHandler(Entry nativeView)
		{
			nativeView.Changed += OnNativeViewChanged;
			nativeView.MotionNotifyEvent += OnNativeViewMotionNotified;
			nativeView.MoveCursor += OnNativeViewCursorMoved;
			nativeView.ButtonPressEvent += OnNativeViewOnButtonPressed;
			nativeView.ButtonReleaseEvent += OnNativeViewOnButtonReleased;
		}

		protected override void DisconnectHandler(Entry nativeView)
		{
			nativeView.Changed -= OnNativeViewChanged;
			nativeView.MotionNotifyEvent -= OnNativeViewMotionNotified;
			nativeView.MoveCursor -= OnNativeViewCursorMoved;
			nativeView.ButtonPressEvent -= OnNativeViewOnButtonPressed;
			nativeView.ButtonReleaseEvent -= OnNativeViewOnButtonReleased;
		}

		(int start, int end) _selectionCache;
		bool _isMouseSelection;

		void HandleSelectionChanged()
		{
			if (NativeView is not { } nativeView || VirtualView is not { } virtualView)
				return;

			var actual = nativeView.GetSelection();

			if (actual != _selectionCache)
			{
				virtualView.OnSelectionLengthChanged(actual);
				_selectionCache = actual;
			}
		}

		void OnNativeViewCursorMoved(object sender, MoveCursorArgs args)
		{
			if (sender != NativeView)
				return;

			NativeView.OnCursorPositionChanged(VirtualView);
			HandleSelectionChanged();
		}

		void OnNativeViewMotionNotified(object sender, MotionNotifyEventArgs args)
		{
			if (sender != NativeView)
				return;

			if (_isMouseSelection)
				HandleSelectionChanged();

		}

		void OnNativeViewOnButtonPressed(object sender, Gtk.ButtonPressEventArgs args)
		{
			if (sender != NativeView)
				return;

			if (args.Event.Button == 1)
			{
				HandleSelectionChanged();
				_isMouseSelection = true;
			}
		}

		void OnNativeViewOnButtonReleased(object o, Gtk.ButtonReleaseEventArgs args)
		{
			if (args.Event.Button == 1)
			{
				HandleSelectionChanged();
				_isMouseSelection = false;
			}
		}

		protected void OnNativeViewChanged(object? sender, EventArgs e)
		{
			if (sender != NativeView)
				return;

			if (NativeView?.OnTextChanged(VirtualView) ?? false)
				HandleSelectionChanged();
		}

		public static void MapText(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateText(entry);
		}

		public static void MapTextColor(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateTextColor(entry.TextColor);
		}

		public static void MapIsPassword(EntryHandler handler, IEntry entry)
		{
			if (handler.NativeView != null && entry.IsPassword)
				handler.NativeView.InputPurpose = InputPurpose.Password;
		}

		public static void MapHorizontalTextAlignment(EntryHandler handler, IEntry entry)
		{
			if (handler.NativeView is { } nativeView)
				nativeView.Alignment = entry.HorizontalTextAlignment.ToXyAlign();
		}

		[MissingMapper]
		public static void MapIsTextPredictionEnabled(EntryHandler handler, IEntry entry) { }

		public static void MapMaxLength(EntryHandler handler, IEntry entry)
		{
			if (handler.NativeView is { } nativeView)
				nativeView.MaxLength = entry.MaxLength;
		}

		public static void MapPlaceholder(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdatePlaceholder(entry);
		}

		public static void MapIsReadOnly(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateIsReadOnly(entry);
		}

		public static void MapFont(EntryHandler handler, IEntry entry)
		{
			handler.MapFont(entry);
		}

		public static void MapCursorPosition(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateCursorPosition(entry);
		}

		public static void MapSelectionLength(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateSelectionLength(entry);
		}

		[MissingMapper]
		public static void MapReturnType(EntryHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapClearButtonVisibility(EntryHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapCharacterSpacing(EntryHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapKeyboard(EntryHandler handler, IEntry entry)
		{
			if (handler.NativeView is not { } nativeView)
				return;

			// https://developer.gnome.org/gtk3/stable/GtkEntry.html#gtk-entry-set-input-purpose
			// seems not to work
			switch (entry.Keyboard)
			{
				case EmailKeyboard:
					nativeView.InputPurpose = InputPurpose.Email;

					break;
				case NumericKeyboard:
					nativeView.InputPurpose = InputPurpose.Number;

					break;
				case TelephoneKeyboard:
					nativeView.InputPurpose = InputPurpose.Phone;

					break;
				case TextKeyboard:
					nativeView.InputPurpose = InputPurpose.FreeForm;

					break;
				case UrlKeyboard:
					nativeView.InputPurpose = InputPurpose.Url;

					break;
				default:
					nativeView.InputPurpose = nativeView.InputPurpose;

					break;
			}
		}

	}

}