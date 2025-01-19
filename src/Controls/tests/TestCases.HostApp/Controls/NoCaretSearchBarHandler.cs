namespace Maui.Controls.Sample
{
	public class NoCaretSearchBarHandler
	{
		public static void RemoveCaret()
		{
			Microsoft.Maui.Handlers.SearchBarHandler.Mapper.AppendToMapping(nameof(NoCaretSearchBar), (handler, view) =>
			{
				if (view is NoCaretSearchBar)
				{
#if ANDROID
					var editText = Microsoft.Maui.Platform.ViewGroupExtensions.GetFirstChildOfType<Android.Widget.EditText>(handler.PlatformView);
					editText.SetCursorVisible(false);
#elif IOS || MACCATALYST
					handler.PlatformView.TintColor = UIKit.UIColor.Clear;
#endif
				}
			});
		}
	}
}
