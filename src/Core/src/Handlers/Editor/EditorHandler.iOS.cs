using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : AbstractViewHandler<IEditor, UITextView>
	{
		protected override UITextView CreateNativeView()
		{
			return new UITextView(CGRect.Empty);
		}

		protected override void SetupDefaults(UITextView nativeView)
		{
			base.SetupDefaults(nativeView);
		}

		public static void MapText(EditorHandler handler, IEditor editor)
		{
			handler.TypedNativeView?.UpdateText(editor);
		}
	}
}