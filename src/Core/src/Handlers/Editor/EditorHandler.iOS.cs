using System;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : AbstractViewHandler<IEditor, UITextView>
	{
		static readonly int BaseHeight = 30;

		protected override UITextView CreateNativeView()
		{
			return new UITextView(CGRect.Empty);
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint) =>
			new SizeRequest(new Size(widthConstraint, BaseHeight));

		public static void MapText(EditorHandler handler, IEditor editor)
		{
			handler.TypedNativeView?.UpdateText(editor);
		}

		public static void MapCharacterSpacing(EditorHandler handler, IEditor editor)
		{
			handler.TypedNativeView?.UpdateCharacterSpacing(editor);
		}
			
		public static void MapIsTextPredictionEnabled(EditorHandler handler, IEditor editor)
		{
			handler.TypedNativeView?.UpdatePredictiveText(editor);
		}
	}
}