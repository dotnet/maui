using Foundation;
using UIKit;

namespace System.Maui.Platform
{
	public partial class EntryRenderer : AbstractViewRenderer<ITextInput, UITextField>
	{
		UIColor _defaultTextColor;

		protected override UITextField CreateView()
		{
			var textView = new UITextField();
			_defaultTextColor = textView.TextColor;

			textView.EditingDidEnd += TextViewEditingDidEnd;

			return textView;
		}

		private void TextViewEditingDidEnd(object sender, EventArgs e) =>
			VirtualView.Text = TypedNativeView.Text ?? string.Empty;

		protected override void DisposeView(UITextField nativeView)
		{
			_defaultTextColor = null;
			nativeView.EditingDidEnd -= TextViewEditingDidEnd;

			base.DisposeView(nativeView);
		}

		public static void MapPropertyColor(IViewRenderer renderer, ITextInput entry)
		{
			(renderer as EntryRenderer)?.UpdateTextColor();
		}

		public static void MapPropertyPlaceholder(IViewRenderer renderer, ITextInput entry)
		{
			if (!(renderer is EntryRenderer entryRenderer))
			{
				return;
			}

			entryRenderer.UpdatePlaceholder();
		}

		public static void MapPropertyPlaceholderColor(IViewRenderer renderer, ITextInput entry) 
		{
			if (!(renderer is EntryRenderer entryRenderer))
			{
				return;
			}

			entryRenderer.UpdatePlaceholder();
		}

		public static void MapPropertyText(IViewRenderer renderer, ITextInput view)
		{
			if (!(renderer.NativeView is UITextField textField))
			{
				return;
			}

			textField.Text = view.Text;
		}

		protected virtual void UpdateTextColor() 
		{
			var color = VirtualView.Color;
			TypedNativeView.TextColor = color.IsDefault ? _defaultTextColor : color.ToNativeColor();
		}

		protected virtual void UpdatePlaceholder()
		{
			var placeholder = VirtualView.Placeholder;

			if (placeholder == null)
			{
				return;
			}

			var targetColor = VirtualView.PlaceholderColor;
			var color = targetColor.IsDefault ? ColorExtensions.SeventyPercentGrey : targetColor.ToNativeColor();

			var attributedPlaceholder = new NSAttributedString(str: placeholder, foregroundColor: color);
			TypedNativeView.AttributedPlaceholder = attributedPlaceholder;
		}
	}
}
