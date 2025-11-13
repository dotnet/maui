using System;
using Foundation;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : ViewHandler<IEntry, MauiTextField>
	{
		readonly MauiTextFieldProxy _proxy = new();

		protected override MauiTextField CreatePlatformView()
		{
			var platformEntry = new MauiTextField
			{
				BorderStyle = UITextBorderStyle.RoundedRect,
				ClipsToBounds = true
			};

			platformEntry.AddMauiDoneAccessoryView(this);
			return platformEntry;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_proxy.SetVirtualView(PlatformView);
		}

		protected override void ConnectHandler(MauiTextField platformView)
		{
			_proxy.Connect(VirtualView, platformView);
		}

		protected override void DisconnectHandler(MauiTextField platformView)
		{
			_proxy.Disconnect(platformView);
		}

		public static void MapText(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateText(entry);

			// Any text update requires that we update any attributed string formatting
			MapFormatting(handler, entry);
		}

		public static void MapTextColor(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateTextColor(entry);
			if (entry.ClearButtonVisibility == ClearButtonVisibility.WhileEditing)
			{
				handler.PlatformView?.UpdateClearButtonColor(entry);
			}
		}

		public static void MapIsPassword(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsPassword(entry);

		public static void MapHorizontalTextAlignment(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateHorizontalTextAlignment(entry);

		public static void MapVerticalTextAlignment(IEntryHandler handler, IEntry entry) =>
			handler?.PlatformView?.UpdateVerticalTextAlignment(entry);

		public static void MapIsTextPredictionEnabled(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsTextPredictionEnabled(entry);

		public static void MapIsSpellCheckEnabled(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsSpellCheckEnabled(entry);

		public static void MapMaxLength(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateMaxLength(entry);

		public static void MapPlaceholder(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdatePlaceholder(entry);

		public static void MapPlaceholderColor(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdatePlaceholder(entry);

		public static void MapIsReadOnly(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateIsReadOnly(entry);

		public static void MapKeyboard(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateKeyboard(entry);

		public static void MapReturnType(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateReturnType(entry);

		public static void MapFont(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateFont(entry, handler.GetRequiredService<IFontManager>());

		public static void MapCharacterSpacing(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateCharacterSpacing(entry);

		public static void MapCursorPosition(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateCursorPosition(entry);

		public static void MapSelectionLength(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateSelectionLength(entry);

		public static void MapClearButtonVisibility(IEntryHandler handler, IEntry entry) =>
			handler.PlatformView?.UpdateClearButtonVisibility(entry);

		public static void MapFormatting(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateMaxLength(entry);

			// Update all of the attributed text formatting properties
			handler.PlatformView?.UpdateCharacterSpacing(entry);

			// Setting any of those may have removed text alignment settings,
			// so we need to make sure those are applied, too
			handler.PlatformView?.UpdateHorizontalTextAlignment(entry);
		}

		protected virtual bool OnShouldReturn(UITextField view) =>
			_proxy.OnShouldReturn(view);

		class MauiTextFieldProxy
		{
			bool _set;
			WeakReference<IEntry>? _virtualView;

			IEntry? VirtualView => _virtualView is not null && _virtualView.TryGetTarget(out var v) ? v : null;

			public void Connect(IEntry virtualView, MauiTextField platformView)
			{
				_virtualView = new(virtualView);

				platformView.ShouldReturn += OnShouldReturn;
				platformView.EditingDidBegin += OnEditingBegan;
				platformView.EditingChanged += OnEditingChanged;
				platformView.EditingDidEnd += OnEditingEnded;
				platformView.TextPropertySet += OnTextPropertySet;
				platformView.ShouldChangeCharacters += OnShouldChangeCharacters;
			}

			public void Disconnect(MauiTextField platformView)
			{
				_virtualView = null;

				platformView.ShouldReturn -= OnShouldReturn;
				platformView.EditingDidBegin -= OnEditingBegan;
				platformView.EditingChanged -= OnEditingChanged;
				platformView.EditingDidEnd -= OnEditingEnded;
				platformView.TextPropertySet -= OnTextPropertySet;
				platformView.ShouldChangeCharacters -= OnShouldChangeCharacters;

				if (_set)
					platformView.SelectionChanged -= OnSelectionChanged;

				_set = false;
			}

			public void SetVirtualView(MauiTextField platformView)
			{
				if (!_set)
					platformView.SelectionChanged += OnSelectionChanged;
				_set = true;
			}

			public bool OnShouldReturn(UITextField view)
			{
				KeyboardAutoManager.GoToNextResponderOrResign(view);

				VirtualView?.Completed();

				return false;
			}

			void OnEditingBegan(object? sender, EventArgs e)
			{
				if (sender is MauiTextField platformView && VirtualView is IEntry virtualView)
				{
					if (virtualView.SelectionLength > 0)
					{
						platformView.UpdateSelectionLength(virtualView);
					}
					else
					{
						platformView.UpdateCursorPosition(virtualView);
					}
					virtualView.IsFocused = true;
				}
			}

			void OnEditingChanged(object? sender, EventArgs e)
			{
				if (sender is MauiTextField platformView)
				{
					// Update cursor position BEFORE updating text so that when TextChanged event fires,
					// the CursorPosition property reflects the current native cursor position
					var cursorPosition = platformView.GetCursorPosition();
					
					if (VirtualView?.CursorPosition != cursorPosition)
					{
						VirtualView?.CursorPosition = cursorPosition;
					}

					VirtualView?.UpdateText(platformView.Text);
				}
			}

			void OnEditingEnded(object? sender, EventArgs e)
			{
				if (sender is MauiTextField platformView && VirtualView is IEntry virtualView)
				{
					virtualView.UpdateText(platformView.Text);
					virtualView.IsFocused = false;
				}
			}

			void OnTextPropertySet(object? sender, EventArgs e)
			{
				if (sender is MauiTextField platformView)
				{
					VirtualView?.UpdateText(platformView.Text);
				}
			}

			bool OnShouldChangeCharacters(UITextField textField, NSRange range, string replacementString) =>
				VirtualView?.TextWithinMaxLength(textField.Text, range, replacementString) ?? false;

			void OnSelectionChanged(object? sender, EventArgs e)
			{
				if (sender is MauiTextField platformView && VirtualView is IEntry virtualView)
				{
					var cursorPosition = platformView.GetCursorPosition();
					var selectedTextLength = platformView.GetSelectedTextLength();

					if (virtualView.CursorPosition != cursorPosition)
						virtualView.CursorPosition = cursorPosition;

					if (virtualView.SelectionLength != selectedTextLength)
						virtualView.SelectionLength = selectedTextLength;
				}
			}
		}
	}
}