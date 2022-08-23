using System;
using Gtk;
using Microsoft.Maui.Graphics.Platform.Gtk;

namespace Microsoft.Maui.Handlers
{

	//https://docs.gtk.org/gtk3/class.Entry.html

	public partial class EntryHandler : ViewHandler<IEntry, Entry>
	{

		protected override Entry CreatePlatformView()
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
			if (PlatformView is not { } nativeView || VirtualView is not { } virtualView)
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
			if (sender != PlatformView)
				return;

			PlatformView.OnCursorPositionChanged(VirtualView);
			HandleSelectionChanged();
		}

		void OnNativeViewMotionNotified(object sender, MotionNotifyEventArgs args)
		{
			if (sender != PlatformView)
				return;

			if (_isMouseSelection)
				HandleSelectionChanged();

		}

		void OnNativeViewOnButtonPressed(object sender, Gtk.ButtonPressEventArgs args)
		{
			if (sender != PlatformView)
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
			if (sender != PlatformView)
				return;

			if (PlatformView?.OnTextChanged(VirtualView) ?? false)
				HandleSelectionChanged();
		}

		public static void MapText(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateText(entry);
		}

		public static void MapTextColor(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateTextColor(entry.TextColor);
		}

		public static void MapIsPassword(IEntryHandler handler, IEntry entry)
		{
			if (handler.PlatformView is { } nativeView)
			{
				nativeView.Visibility = !entry.IsPassword;
			}
		}

		public static void MapHorizontalTextAlignment(IEntryHandler handler, IEntry entry)
		{
			if (handler.PlatformView is { } nativeView)
				nativeView.Alignment = entry.HorizontalTextAlignment.ToXyAlign();
		}

		[MissingMapper]
		public static void MapVerticalTextAlignment(IEntryHandler handler, IEntry entry)
		{ }

		[MissingMapper]
		public static void MapIsTextPredictionEnabled(IEntryHandler handler, IEntry entry) { }

		public static void MapMaxLength(IEntryHandler handler, IEntry entry)
		{
			if (handler.PlatformView is { } nativeView)
				nativeView.MaxLength = entry.MaxLength;
		}

		public static void MapPlaceholder(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdatePlaceholder(entry);
		}

		[MissingMapper]
		public static void MapPlaceholderColor(IEntryHandler handler, IEntry entry) { }

		public static void MapIsReadOnly(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateIsReadOnly(entry);
		}

		public static void MapFont(IEntryHandler handler, IEntry entry)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(entry, fontManager);
		}

		public static void MapCursorPosition(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateCursorPosition(entry);
		}

		public static void MapSelectionLength(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateSelectionLength(entry);
		}

		[MissingMapper]
		public static void MapReturnType(IEntryHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapClearButtonVisibility(IEntryHandler handler, IEntry entry) { }

		public static void MapCharacterSpacing(IEntryHandler handler, IEntry entry)
		{
			if (handler.PlatformView is not { } nativeView)
				return;

			nativeView.Attributes = nativeView.Attributes.AttrListFor(entry.CharacterSpacing);
		}

		[MissingMapper]
		public static void MapKeyboard(IEntryHandler handler, IEntry entry)
		{
			if (handler.PlatformView is not { } nativeView)
				return;

			// https://docs.gtk.org/gtk3/method.Entry.set_input_purpose.html
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