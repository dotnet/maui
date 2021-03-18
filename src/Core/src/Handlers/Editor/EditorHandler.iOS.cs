using CoreGraphics;
using UIKit;
using Microsoft.Maui.Platform.iOS;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : AbstractViewHandler<IEditor, MauiTextView>
	{
		static readonly int BaseHeight = 30;
	
		static readonly UIColor DefaultPlaceholderColor = ColorExtensions.PlaceholderColor;

		protected override MauiTextView CreateNativeView()
		{
			return new MauiTextView(CGRect.Empty);
		}

		protected override void SetupDefaults(MauiTextView nativeView) 
		{
			nativeView.PlaceholderTextColor = DefaultPlaceholderColor;
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint) =>
			new SizeRequest(new Size(widthConstraint, BaseHeight));

		protected override void ConnectHandler(MauiTextView nativeView)
		{
			nativeView.Changed += OnChanged;
		}

		protected override void DisconnectHandler(MauiTextView nativeView)
		{
			nativeView.Changed -= OnChanged;
		}

		void OnChanged(object? sender, System.EventArgs e) => OnTextChanged();

		void OnTextChanged()
		{
			if (TypedNativeView == null)
				return;

			TypedNativeView.HidePlaceholder(!string.IsNullOrEmpty(TypedNativeView.Text));
		}

		public static void MapText(EditorHandler handler, IEditor editor)
		{
			handler.TypedNativeView?.UpdateText(editor);
		}

		public static void MapPlaceholder(EditorHandler handler, IEditor editor)
		{
			handler.TypedNativeView?.UpdatePlaceholder(editor);
		}

		public static void MapPlaceholderColor(EditorHandler handler, IEditor editor) 
		{
			handler.TypedNativeView?.UpdatePlaceholderColor(editor, DefaultPlaceholderColor);
		}
	}
}
