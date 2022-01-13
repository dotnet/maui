using System;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.NUI;
using NView = Tizen.NUI.BaseComponents.View;
using TSize = Tizen.UIExtensions.Common.Size;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : ViewHandler<IEntry, Entry>
	{
		// It is workaround, Tizen.UIExtension should be updated
		class MauiEntry : Entry, IMeasurable
		{
			TSize IMeasurable.Measure(double availableWidth, double availableHeight)
			{
				if (!string.IsNullOrEmpty(Text))
				{
					if (availableWidth < NaturalSize.Width)
					{
						return new TSize(availableWidth, NaturalSize.Height);
					}
					else if (NaturalSize.Width > 0)
					{
						return new TSize(NaturalSize.Width, NaturalSize.Height);
					}
					else
					{
						// even though text but natural size is zero. it is abnormal state
						return new TSize(Math.Max(Text.Length * PixelSize + 10, availableWidth), PixelSize + 10);
					}
				}
				else
				{
					return new TSize(Math.Max(PixelSize + 10, availableWidth), PixelSize + 10);
				};
			}
		}

		protected override Entry CreatePlatformView() => new MauiEntry();

		protected override void ConnectHandler(Entry nativeView)
		{
			nativeView.KeyEvent += OnKeyEvent;
			nativeView.CursorPositionChanged += OnCursorPositionChanged;
			nativeView.SelectionChanged += OnSelectionChanged;
			nativeView.SelectionCleared += OnSelectionCleared;
			nativeView.TextChanged += OnTextChanged;
			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(Entry nativeView)
		{
			nativeView.KeyEvent -= OnKeyEvent;
			nativeView.CursorPositionChanged -= OnCursorPositionChanged;
			nativeView.SelectionChanged -= OnSelectionChanged;
			nativeView.SelectionCleared -= OnSelectionCleared;
			nativeView.TextChanged -= OnTextChanged;
			base.DisconnectHandler(nativeView);
		}

		public static void MapBackground(IEntryHandler handler, IEntry entry)
		{
			handler.UpdateValue(nameof(handler.ContainerView));
			handler.ToPlatform()?.UpdateBackground(entry);
		}

		public static void MapText(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateText(entry);

		public static void MapTextColor(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateTextColor(entry);

		public static void MapIsPassword(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsPassword(entry);

		public static void MapHorizontalTextAlignment(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateHorizontalTextAlignment(entry);

		public static void MapVerticalTextAlignment(EntryHandler handler, IEntry entry) =>
			handler?.PlatformView?.UpdateVerticalTextAlignment(entry);

		public static void MapIsTextPredictionEnabled(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsTextPredictionEnabled(entry);

		public static void MapMaxLength(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateMaxLength(entry);

		public static void MapPlaceholder(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdatePlaceholder(entry);

		public static void MapPlaceholderColor(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdatePlaceholderColor(entry);

		public static void MapFont(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateFont(entry, handler.GetRequiredService<IFontManager>());

		public static void MapIsReadOnly(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsReadOnly(entry);

		public static void MapKeyboard(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateKeyboard(entry);

		public static void MapReturnType(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateReturnType(entry);

		[MissingMapper]
		public static void MapCharacterSpacing(EntryHandler handler, IEntry entry) { }

		public static void MapCursorPosition(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateCursorPosition(entry);

		public static void MapSelectionLength(EntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateSelectionLength(entry);

		[MissingMapper]
		public static void MapClearButtonVisibility(EntryHandler handler, IEntry entry) { }

		bool OnKeyEvent(object? source, NView.KeyEventArgs e)
		{
			if (VirtualView == null || PlatformView == null)
				return false;

			if (e.Key.State == Key.StateType.Down && e.Key.KeyPressedName == "Return")
			{
				VirtualView.Completed();
				return true;
			}
			return false;
		}

		void OnSelectionChanged(object? sender, EventArgs e)
		{
			if (VirtualView == null || PlatformView == null)
				return;

			VirtualView.SelectionLength = Math.Abs(PlatformView.SelectedTextEnd - PlatformView.SelectedTextStart);
			VirtualView.CursorPosition = Math.Min(PlatformView.SelectedTextEnd, PlatformView.SelectedTextStart);
		}

		void OnCursorPositionChanged(object? sender, EventArgs e)
		{
			if (VirtualView == null || NativeView == null)
				return;

			VirtualView.CursorPosition = PlatformView.PrimaryCursorPosition;
		}

		void OnSelectionCleared(object? sender, EventArgs e)
		{
			if (VirtualView == null || PlatformView == null)
				return;

			VirtualView.SelectionLength = 0;
			VirtualView.CursorPosition = PlatformView.PrimaryCursorPosition;
		}

		void OnTextChanged(object? sender, TextField.TextChangedEventArgs e)
		{
			if (VirtualView == null || PlatformView == null)
				return;

			VirtualView.Text = PlatformView.Text;
		}
	}
}