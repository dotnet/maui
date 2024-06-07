#nullable enable
using System;
using Microsoft.Maui.Devices;
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
		bool _set;

		protected override TextBox CreatePlatformView() =>
			new MauiPasswordTextBox()
			{
				MauiPasswordTextBox.IsObfuscationDelayed = s_shouldBeDelayed
			};

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			if (!_set)
				PlatformView.SelectionChanged += OnPlatformSelectionChanged;

			_set = true;
		}

		protected override void ConnectHandler(TextBox platformView)
		{
			platformView.KeyUp += OnPlatformKeyUp;
			platformView.TextChanged += OnPlatformTextChanged;
			platformView.SizeChanged += OnPlatformViewSizeChanged;
		}

		protected override void DisconnectHandler(TextBox platformView)
		{
			platformView.SizeChanged -= OnPlatformViewSizeChanged;
			platformView.KeyUp -= OnPlatformKeyUp;
			platformView.TextChanged -= OnPlatformTextChanged;

			if (_set)
				platformView.SelectionChanged -= OnPlatformSelectionChanged;

			_set = false;
		}

		public static void MapText(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateText(entry);

		public static void MapIsPassword(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsPassword(entry);

		public static void MapBackground(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateBackground(entry);

		public static void MapTextColor(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateTextColor(entry);

		public static void MapHorizontalTextAlignment(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateHorizontalTextAlignment(entry);

		public static void MapVerticalTextAlignment(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateVerticalTextAlignment(entry);

		public static void MapIsTextPredictionEnabled(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsTextPredictionEnabled(entry);

		public static void MapIsSpellCheckEnabled(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsSpellCheckEnabled(entry);

		public static void MapMaxLength(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateMaxLength(entry);

		public static void MapPlaceholder(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdatePlaceholder(entry);

		public static void MapPlaceholderColor(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdatePlaceholderColor(entry);

		public static void MapIsReadOnly(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsReadOnly(entry);

		public static void MapFont(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateFont(entry, handler.GetRequiredService<IFontManager>());

		public static void MapReturnType(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateReturnType(entry);

		public static void MapClearButtonVisibility(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateClearButtonVisibility(entry);

		public static void MapCharacterSpacing(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateCharacterSpacing(entry);

		public static void MapKeyboard(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateKeyboard(entry);

		public static void MapCursorPosition(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateCursorPosition(entry);

		public static void MapSelectionLength(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateSelectionLength(entry);

		void OnPlatformTextChanged(object sender, TextChangedEventArgs args)
		{
			if (PlatformView is MauiPasswordTextBox passwordBox)
				VirtualView?.UpdateText(MauiPasswordTextBox.Password);
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

			VirtualView?.Completed();
		}

		void OnPlatformSelectionChanged(object sender, RoutedEventArgs e)
		{
			var cursorPosition = PlatformView.GetCursorPosition();
			var selectedTextLength = PlatformView.SelectionLength;

			if (VirtualView.CursorPosition != cursorPosition)
				VirtualView.CursorPosition = cursorPosition;

			if (VirtualView.SelectionLength != selectedTextLength)
				VirtualView.SelectionLength = selectedTextLength;
		}

		void OnPlatformViewSizeChanged(object sender, SizeChangedEventArgs e) =>
			MauiTextBox.InvalidateAttachedProperties(PlatformView);
	}
}