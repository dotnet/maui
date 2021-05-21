using System;
using Gtk;

namespace Microsoft.Maui.Handlers
{

	public partial class EntryHandler : ViewHandler<IEntry, Entry>
	{

		protected override Entry CreateNativeView()
		{
			return new();
		}

		protected override void ConnectHandler(Entry nativeView)
		{
			nativeView.Changed += OnNativeViewChanged;
		}

		protected override void DisconnectHandler(Entry nativeView)
		{
			nativeView.Changed -= OnNativeViewChanged;
		}

		protected void OnNativeViewChanged(object? sender, EventArgs e)
		{
			if (sender != NativeView)
				return;

			NativeView?.OnTextChanged(VirtualView);
		}

		public static void MapText(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateText(entry);
		}

		public static void MapTextColor(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateTextColor(entry.TextColor);
		}

		public static void MapIsPassword(EntryHandler handler, IEntry entry)
		{
			if (handler.NativeView != null && entry.IsPassword)
				handler.NativeView.InputPurpose = InputPurpose.Password;
		}

		public static void MapHorizontalTextAlignment(EntryHandler handler, IEntry entry)
		{
			if (handler.NativeView is { } nativeView)
				nativeView.Alignment = entry.HorizontalTextAlignment.ToXyAlign();
		}

		[MissingMapper]
		public static void MapIsTextPredictionEnabled(EntryHandler handler, IEntry entry) { }

		public static void MapMaxLength(EntryHandler handler, IEntry entry)
		{
			if (handler.NativeView is { } nativeView)
				nativeView.MaxLength = entry.MaxLength;
		}

		public static void MapPlaceholder(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdatePlaceholder(entry);
		}

		public static void MapIsReadOnly(EntryHandler handler, IEntry entry)
		{
			handler.NativeView?.UpdateIsReadOnly(entry);
		}

		public static void MapFont(EntryHandler handler, IEntry entry)
		{
			handler.MapFont(entry);
		}

		[MissingMapper]
		public static void MapReturnType(EntryHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapClearButtonVisibility(EntryHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapCharacterSpacing(EntryHandler handler, IEntry entry) { }

		[MissingMapper]
		public static void MapKeyboard(EntryHandler handler, IEntry entry) { }

	}

}