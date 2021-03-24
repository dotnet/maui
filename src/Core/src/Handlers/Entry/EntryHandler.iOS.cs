using System;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Platform.iOS;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : AbstractViewHandler<IEntry, MauiTextField>
	{
		static readonly int BaseHeight = 30;

		static UIColor? DefaultTextColor;

		protected override MauiTextField CreateNativeView()
		{
			return new MauiTextField
			{
				BorderStyle = UITextBorderStyle.RoundedRect,
				ClipsToBounds = true
			};
		}

		protected override void ConnectHandler(MauiTextField nativeView)
		{
			nativeView.EditingChanged += OnEditingChanged;
			nativeView.EditingDidEnd += OnEditingEnded;
			nativeView.TextPropertySet += OnTextPropertySet;
			nativeView.ShouldChangeCharacters += OnShouldChangeCharacters;
		}

		protected override void DisconnectHandler(MauiTextField nativeView)
		{
			nativeView.EditingChanged -= OnEditingChanged;
			nativeView.EditingDidEnd -= OnEditingEnded;
			nativeView.TextPropertySet -= OnTextPropertySet;
			nativeView.ShouldChangeCharacters -= OnShouldChangeCharacters;
		}

		protected override void SetupDefaults(MauiTextField nativeView)
		{
			DefaultTextColor = nativeView.TextColor;
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint) =>
			new SizeRequest(new Size(widthConstraint, BaseHeight));

		public static void MapText(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateText(entry);

			// Any text update requires that we update any attributed string formatting
			MapFormatting(handler, entry);
		}

		public static void MapTextColor(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateTextColor(entry, DefaultTextColor);
		}

		public static void MapIsPassword(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateIsPassword(entry);
		}

		public static void MapHorizontalTextAlignment(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateHorizontalTextAlignment(entry);
		}

		public static void MapIsTextPredictionEnabled(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateIsTextPredictionEnabled(entry);
		}

		public static void MapMaxLength(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateMaxLength(entry);
		}

		public static void MapPlaceholder(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdatePlaceholder(entry);
		}

		public static void MapIsReadOnly(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateIsReadOnly(entry);
		}

		public static void MapReturnType(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateReturnType(entry);
		}

		public static void MapFont(EntryHandler handler, IEntry entry)
		{
			_ = handler.Services ?? throw new InvalidOperationException($"{nameof(Services)} should have been set by base class.");

			var fontManager = handler.Services.GetRequiredService<IFontManager>();

			handler.TypedNativeView?.UpdateFont(entry, fontManager);
		}

		public static void MapFormatting(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateMaxLength(entry);

			// Update all of the attributed text formatting properties
			handler.TypedNativeView?.UpdateCharacterSpacing(entry);

			// Setting any of those may have removed text alignment settings,
			// so we need to make sure those are applied, too
			handler.TypedNativeView?.UpdateHorizontalTextAlignment(entry);
		}

		public static void MapCharacterSpacing(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateCharacterSpacing(entry);
		}

		public static void MapClearButtonVisibility(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateClearButtonVisibility(entry);
		}

		void OnEditingChanged(object? sender, EventArgs e) => OnTextChanged();

		void OnEditingEnded(object? sender, EventArgs e) => OnTextChanged();

		void OnTextPropertySet(object? sender, EventArgs e) => OnTextChanged();

		void OnTextChanged()
		{
			if (VirtualView == null || TypedNativeView == null)
				return;

			// Even though <null> is technically different to "", it has no
			// functional difference to apps. Thus, hide it.
			var mauiText = VirtualView!.Text ?? string.Empty;
			var nativeText = TypedNativeView.Text ?? string.Empty;
			if (mauiText != nativeText)
				VirtualView.Text = nativeText;
		}

		bool OnShouldChangeCharacters(UITextField textField, NSRange range, string replacementString)
		{
			var currLength = textField?.Text?.Length ?? 0;

			// fix a crash on undo
			if (range.Length + range.Location > currLength)
				return false;

			if (VirtualView == null || TypedNativeView == null)
				return false;

			var addLength = replacementString?.Length ?? 0;
			var remLength = range.Length;

			var newLength = currLength + addLength - remLength;

			return newLength <= VirtualView.MaxLength;
		}
	}
}