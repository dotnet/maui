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
				}
				;
			}
		}

		protected override Entry CreatePlatformView() => new MauiEntry
		{
			Focusable = true,
			FocusableInTouch = true,
		};

		protected override void ConnectHandler(Entry platformView)
		{
			platformView.KeyEvent += OnKeyEvent;
			platformView.CursorPositionChanged += OnCursorPositionChanged;
			platformView.SelectionChanged += OnSelectionChanged;
			platformView.SelectionCleared += OnSelectionCleared;
			platformView.TextChanged += OnTextChanged;
			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(Entry platformView)
		{
			if (!platformView.HasBody())
				return;

			platformView.KeyEvent -= OnKeyEvent;
			platformView.CursorPositionChanged -= OnCursorPositionChanged;
			platformView.SelectionChanged -= OnSelectionChanged;
			platformView.SelectionCleared -= OnSelectionCleared;
			platformView.TextChanged -= OnTextChanged;
			base.DisconnectHandler(platformView);
		}

		public static void MapBackground(IEntryHandler handler, IEntry entry)
		{
			handler.UpdateValue(nameof(handler.ContainerView));
			handler.ToPlatform()?.UpdateBackground(entry);
		}

		public static void MapText(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateText(entry);

		public static void MapTextColor(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateTextColor(entry);

		public static void MapIsPassword(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsPassword(entry);

		public static void MapHorizontalTextAlignment(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateHorizontalTextAlignment(entry);

		public static void MapVerticalTextAlignment(IEntryHandler handler, IEntry entry) =>
			handler?.PlatformView?.UpdateVerticalTextAlignment(entry);

		public static void MapIsTextPredictionEnabled(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsTextPredictionEnabled(entry);

		[MissingMapper]
		public static void MapIsSpellCheckEnabled(IEntryHandler handler, IEntry entry) { }

		public static void MapMaxLength(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateMaxLength(entry);

		public static void MapPlaceholder(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdatePlaceholder(entry);

		public static void MapPlaceholderColor(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdatePlaceholderColor(entry);

		public static void MapFont(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateFont(entry, handler.GetRequiredService<IFontManager>());

		public static void MapIsReadOnly(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsReadOnly(entry);

		public static void MapKeyboard(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateKeyboard(entry);

		public static void MapReturnType(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateReturnType(entry);

		public static void MapCharacterSpacing(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView.UpdateCharacterSpacing(entry);

		public static void MapCursorPosition(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateCursorPosition(entry);

		public static void MapSelectionLength(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateSelectionLength(entry);

		[MissingMapper]
		public static void MapClearButtonVisibility(IEntryHandler handler, IEntry entry) { }

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
			if (VirtualView == null || PlatformView == null)
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