#nullable enable
using System;
using Microsoft.Maui.Essentials;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.System;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : ViewHandler<IEntry, TextBox>
	{
		static readonly bool s_shouldBeDelayed = DeviceInfo.Idiom != DeviceIdiom.Desktop;

		protected override TextBox CreatePlatformView() =>
			new MauiPasswordTextBox()
			{
				IsObfuscationDelayed = s_shouldBeDelayed
			};

		protected override void ConnectHandler(TextBox platformView)
		{
			platformView.KeyUp += OnPlatformKeyUp;
			platformView.TextChanged += OnPlatformTextChanged;
			platformView.SelectionChanged += OnPlatformSelectionChanged;
			platformView.Loaded += OnPlatformLoaded;
		}

		protected override void DisconnectHandler(TextBox platformView)
		{
			platformView.Loaded -= OnPlatformLoaded;
			platformView.KeyUp -= OnPlatformKeyUp;
			platformView.TextChanged -= OnPlatformTextChanged;
			platformView.SelectionChanged -= OnPlatformSelectionChanged;
		}

		public static void MapText(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateText(entry);

		public static void MapIsPassword(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsPassword(entry);

		public static void MapBackground(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateBackground(entry);

		public static void MapTextColor(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateTextColor(entry);

		public static void MapHorizontalTextAlignment(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateHorizontalTextAlignment(entry);

		public static void MapVerticalTextAlignment(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateVerticalTextAlignment(entry);

		public static void MapIsTextPredictionEnabled(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsTextPredictionEnabled(entry);

		public static void MapMaxLength(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateMaxLength(entry);

		public static void MapPlaceholder(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdatePlaceholder(entry);

		public static void MapPlaceholderColor(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdatePlaceholderColor(entry);

		public static void MapIsReadOnly(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsReadOnly(entry);

		public static void MapFont(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateFont(entry, handler.GetRequiredService<IFontManager>());

		public static void MapReturnType(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateReturnType(entry);

		public static void MapClearButtonVisibility(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateClearButtonVisibility(entry);

		public static void MapCharacterSpacing(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateCharacterSpacing(entry);

		public static void MapKeyboard(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateKeyboard(entry);

		public static void MapCursorPosition(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateCursorPosition(entry);

		public static void MapSelectionLength(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateSelectionLength(entry);

		void OnPlatformTextChanged(object sender, TextChangedEventArgs args)
		{
			if (PlatformView is MauiPasswordTextBox passwordBox)
				VirtualView?.UpdateText(passwordBox.Password);
			else
				VirtualView?.UpdateText(PlatformView.Text);
		}

		void OnPlatformKeyUp(object? sender, KeyRoutedEventArgs args)
		{
			if (args?.Key != VirtualKey.Enter)
				return;

			if (VirtualView?.ReturnType == ReturnType.Next)
			{
				PlatformView?.TryMoveFocus(FocusNavigationDirection.Next);
			}
			else
			{
				// TODO: Hide the soft keyboard; this matches the behavior of .NET MAUI on Android/iOS
			}

			VirtualView?.Completed();
		}

		void OnPlatformSelectionChanged(object sender, RoutedEventArgs e)
		{
			if (VirtualView.CursorPosition != PlatformView.SelectionStart)
				VirtualView.CursorPosition = PlatformView.SelectionStart;

			if (VirtualView.SelectionLength != PlatformView.SelectionLength)
				VirtualView.SelectionLength = PlatformView.SelectionLength;
		}

		void OnPlatformLoaded(object sender, RoutedEventArgs e) =>
			MauiTextBox.InvalidateAttachedProperties(PlatformView);
	}
}