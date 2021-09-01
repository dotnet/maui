#nullable enable
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.System;

﻿namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : ViewHandler<IEntry, MauiTextBox>
	{
		Brush? _defaultPplaceholderBrush;
		Brush? _defaultPlaceholderColorFocusBrush;

		protected override MauiTextBox CreateNativeView()
		{
			var nativeEntry = new MauiTextBox { Style = UI.Xaml.Application.Current.Resources["MauiTextBoxStyle"] as UI.Xaml.Style };
			
			_defaultPplaceholderBrush = nativeEntry.PlaceholderForeground;
			_defaultPlaceholderColorFocusBrush = nativeEntry.PlaceholderForegroundFocusBrush;

			return nativeEntry;
		}

		protected override void ConnectHandler(MauiTextBox nativeView)
		{
			nativeView.KeyUp += OnNativeKeyUp;
		}

		protected override void DisconnectHandler(MauiTextBox nativeView)
		{
			nativeView.KeyUp -= OnNativeKeyUp;
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

		[MissingMapper]
		public static void MapIsTextPredictionEnabled(IViewHandler handler, IEntry entry) { }

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
			handler.NativeView?.UpdatePlaceholderColor(entry, handler._defaultPplaceholderBrush, handler._defaultPlaceholderColorFocusBrush);
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

		[MissingMapper]
		public static void MapKeyboard(IViewHandler handler, IEntry entry) { }

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

		[MissingMapper]
		public static void MapCursorPosition(IViewHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapSelectionLength(IViewHandler handler, IEntry entry) { }
	}
}