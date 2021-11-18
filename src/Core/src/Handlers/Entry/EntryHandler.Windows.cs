#nullable enable
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : ViewHandler<IEntry, TextBox>
	{
		protected override TextBox CreateNativeView() => new MauiTextBox();

		protected override void ConnectHandler(TextBox nativeView)
		{
			nativeView.KeyUp += OnNativeKeyUp;
			nativeView.TextChanged += TextChanged;
			nativeView.SelectionChanged += OnSelectionChanged;
		}

		protected override void DisconnectHandler(TextBox nativeView)
		{
			nativeView.KeyUp -= OnNativeKeyUp;
			nativeView.TextChanged -= TextChanged;
			nativeView.SelectionChanged -= OnSelectionChanged;
		}

		public static void MapText(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateText(entry);
		}

		public static void MapBackground(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateBackground(entry);
		}

		public static void MapTextColor(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateTextColor(entry);
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
			handler.NativeView?.UpdatePlaceholderColor(entry);
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

		public static void MapCursorPosition(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateCursorPosition(entry);
		}

		public static void MapSelectionLength(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateSelectionLength(entry);
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

		void TextChanged(object sender, TextChangedEventArgs args)
		{
			VirtualView?.UpdateText(NativeView.Text);
		}

		void OnSelectionChanged(object sender, RoutedEventArgs e)
		{
			if (VirtualView.CursorPosition != NativeView.SelectionStart)
				VirtualView.CursorPosition = NativeView.SelectionStart;

			if (VirtualView.SelectionLength != NativeView.SelectionLength)
				VirtualView.SelectionLength = NativeView.SelectionLength;
		}
	}
}