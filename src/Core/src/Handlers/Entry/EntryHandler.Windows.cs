#nullable enable
using Microsoft.UI.Xaml.Input;
using Windows.System;

﻿namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : ViewHandler<IEntry, MauiTextBox>
	{
		protected override MauiTextBox CreateNativeView() =>
			new MauiTextBox { Style = UI.Xaml.Application.Current.Resources["MauiTextBoxStyle"] as UI.Xaml.Style };

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

		[MissingMapper]
		public static void MapTextColor(IViewHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapIsPassword(IViewHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapHorizontalTextAlignment(IViewHandler handler, IEntry entry) { }

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

		public static void MapIsReadOnly(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateIsReadOnly(entry);
		}

		[MissingMapper]
		public static void MapFont(IViewHandler handler, IEntry entry) { }

		public static void MapReturnType(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateReturnType(entry);
		}

		[MissingMapper]
		public static void MapClearButtonVisibility(IViewHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapCharacterSpacing(IViewHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapKeyboard(IViewHandler handler, IEntry entry) { }

		void OnNativeKeyUp(object? sender, KeyRoutedEventArgs args)
		{
			if (args?.Key != VirtualKey.Enter)
				return;

			if (VirtualView?.ReturnType == ReturnType.Next)
			{
				FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
			}
			else
			{
				// TODO: Hide the soft keyboard; this matches the behavior of .NET MAUI on Android/iOS
			}

			VirtualView?.Completed();
		}
	}
}