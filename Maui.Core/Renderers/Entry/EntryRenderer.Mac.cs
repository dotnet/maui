
using AppKit;

namespace System.Maui.Platform
{
	public partial class EntryRenderer : AbstractViewRenderer<ITextInput, NSTextView>
	{
		protected override NSTextView CreateView()
		{
			return new NSTextView();
		}

		public static void MapPropertyColor(IViewRenderer renderer, ITextInput entry)
		{
			
		}

		public static void MapPropertyPlaceholder(IViewRenderer renderer, ITextInput entry)
		{
			
		}

		public static void MapPropertyPlaceholderColor(IViewRenderer renderer, ITextInput entry) 
		{
			
		}

		public static void MapPropertyText(IViewRenderer renderer, ITextInput view)
		{
			var textField = renderer.NativeView as NSTextField;

			if (textField is null)
			{
				return;
			}

			textField.SetText(view.Text);
		}
	}
}
