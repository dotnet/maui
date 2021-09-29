#nullable enable
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.System;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : ViewHandler<IEntry, MauiTextBox>
	{
		Brush? _defaultPlaceholderBrush;
		Brush? _defaultPlaceholderColorFocusBrush;
	
		protected override MauiTextBox CreateNativeView()
		{
			var nativeEntry = new MauiTextBox { Style = UI.Xaml.Application.Current.Resources["MauiTextBoxStyle"] as UI.Xaml.Style };

			_defaultPlaceholderBrush = nativeEntry.PlaceholderForeground;
			_defaultPlaceholderColorFocusBrush = nativeEntry.PlaceholderForegroundFocusBrush;

			return nativeEntry;
		}

		protected override void ConnectHandler(MauiTextBox nativeView)
		{
			NativeView.CursorPositionChangePending = VirtualView.CursorPosition > 0;
			NativeView.SelectionLengthChangePending = VirtualView.SelectionLength > 0;
			nativeView.KeyUp += OnNativeKeyUp;
			nativeView.CursorPositionChanged += OnCursorPositionChanged;
			nativeView.SelectionLengthChanged += OnSelectionLengthChanged;
		}

		protected override void DisconnectHandler(MauiTextBox nativeView)
		{
			nativeView.KeyUp -= OnNativeKeyUp;
			nativeView.CursorPositionChanged -= OnCursorPositionChanged;
			nativeView.SelectionLengthChanged -= OnSelectionLengthChanged;
		}

		public static void MapText(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateText(entry);
		}

		public static void MapTextColor(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateTextColor(entry);
		}

		public static void MapIsPassword(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateIsPassword(entry);
		}

		public static void MapHorizontalTextAlignment(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateHorizontalTextAlignment(entry);
		}

		public static void MapVerticalTextAlignment(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateVerticalTextAlignment(entry);
		}

		public static void MapIsTextPredictionEnabled(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateIsTextPredictionEnabled(entry);
		}

		public static void MapMaxLength(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateMaxLength(entry);
		}

		public static void MapPlaceholder(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdatePlaceholder(entry);
		}

		public static void MapPlaceholderColor(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdatePlaceholderColor(entry, handler._defaultPlaceholderBrush, handler._defaultPlaceholderColorFocusBrush);
		}

		public static void MapIsReadOnly(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateIsReadOnly(entry);
		}

		public static void MapFont(EntryHandler handler, IEntry entry)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.NativeView?.UpdateFont(entry, fontManager);
		}

		public static void MapReturnType(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateReturnType(entry);
		}

		public static void MapClearButtonVisibility(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateClearButtonVisibility(entry);
		}

		public static void MapCharacterSpacing(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateCharacterSpacing(entry);
		}

		public static void MapKeyboard(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateKeyboard(entry);
		}

		void OnNativeKeyUp(object? sender, KeyRoutedEventArgs args)
		{
			if (args?.Key != VirtualKey.Enter)
				return;

			if (VirtualView?.ReturnType == ReturnType.Next)
			{
				NativeView?.TryMoveFocus(FocusNavigationDirection.Next);
			}
			else
			{
				// TODO: Hide the soft keyboard; this matches the behavior of .NET MAUI on Android/iOS
			}

			VirtualView?.Completed();
		}

		public static void MapCursorPosition(EntryHandler handler, IEntry entry)
		{
			handler.NativeView.CursorPosition = entry.CursorPosition;
		}

		public static void MapSelectionLength(EntryHandler handler, IEntry entry)
		{
			handler.NativeView.ViewSelectionLength = entry.SelectionLength;
		}

		void OnCursorPositionChanged(object? sender, EventArgs e)
		{
			VirtualView.CursorPosition = NativeView.CursorPosition;
		}

		void OnSelectionLengthChanged(object? sender, EventArgs e)
		{
			VirtualView.SelectionLength = NativeView.ViewSelectionLength;
		}

	}
}