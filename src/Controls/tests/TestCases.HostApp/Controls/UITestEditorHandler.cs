using Microsoft.Maui.Handlers;

namespace Maui.Controls.Sample
{
	public class UITestEditorHandler : EditorHandler
	{
		public UITestEditorHandler() : base(EditorHandler.Mapper)
		{
			Mapper.AppendToMapping(nameof(IUITestEditor.IsCursorVisible), (handler, editor) =>
			{
				if (editor is UITestEditor testEditor)
				{
					bool isCursorVisible = testEditor.IsCursorVisible;
#if ANDROID
					handler.PlatformView.SetCursorVisible(isCursorVisible);
#elif IOS || MACCATALYST
					if (isCursorVisible)
						handler.PlatformView.TintColor = UIKit.UITextField.Appearance.TintColor;
					else
						handler.PlatformView.TintColor = UIKit.UIColor.Clear;
#endif
				}
			});
		}
	}
}