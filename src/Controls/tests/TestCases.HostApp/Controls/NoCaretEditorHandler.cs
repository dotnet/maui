namespace Maui.Controls.Sample
{
	public class NoCaretEditorHandler
	{
		public static void RemoveCaret()
		{
			Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping(nameof(NoCaretEditor), (handler, view) =>
			{
				if (view is NoCaretEditor)
				{
#if ANDROID
					handler.PlatformView.SetCursorVisible(false);
#elif IOS
					handler.PlatformView.TintColor = UIKit.UIColor.Clear;
#endif
				}
			});
		}
	}
}