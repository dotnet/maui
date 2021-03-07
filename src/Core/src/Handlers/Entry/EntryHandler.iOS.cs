using System;
using System;
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
		}

		protected override void DisconnectHandler(MauiTextField nativeView)
		{
			nativeView.EditingChanged -= OnEditingChanged;
			nativeView.EditingDidEnd -= OnEditingEnded;
			nativeView.TextPropertySet -= OnTextPropertySet;
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
		}

		public static void MapTextColor(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateTextColor(entry, DefaultTextColor);
		}

		public static void MapIsPassword(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateIsPassword(entry);
		}

		public static void MapIsTextPredictionEnabled(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateIsTextPredictionEnabled(entry);
		}

		public static void MapPlaceholder(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdatePlaceholder(entry);
		}

		public static void MapIsReadOnly(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateIsReadOnly(entry);
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
			var mauiText = VirtualView.Text ?? string.Empty;
			var nativeText = TypedNativeView.Text ?? string.Empty;
			if (mauiText != nativeText)
				VirtualView.Text = nativeText;
		}
		
		public static void MapFont(EntryHandler handler, IEntry entry)
		{
			var services = App.Current?.Services
				?? throw new InvalidOperationException($"Unable to find service provider, the App.Current.Services was null.");
			var fontManager = services.GetRequiredService<IFontManager>();

			handler.TypedNativeView?.UpdateFont(entry, fontManager);
		}
	}
}