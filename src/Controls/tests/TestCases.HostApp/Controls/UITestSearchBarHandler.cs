using Microsoft.Maui.Handlers;

namespace Maui.Controls.Sample
{
	public class UITestSearchBarHandler : SearchBarHandler
	{
		public UITestSearchBarHandler() : base(SearchBarHandler.Mapper)
		{
			Mapper.AppendToMapping(nameof(UITestSearchBar.IsCursorVisible), (handler, searchBar) =>
			{
				if (searchBar is UITestSearchBar testSearchBar)
				{
					bool isCursorVisible = testSearchBar.IsCursorVisible;
#if ANDROID
					var editText = Microsoft.Maui.Platform.ViewGroupExtensions.GetFirstChildOfType<Android.Widget.EditText>(handler.PlatformView);
					editText.SetCursorVisible(isCursorVisible);
#elif IOS || MACCATALYST
					if (isCursorVisible)
						handler.QueryEditor.TintColor = UIKit.UITextField.Appearance.TintColor;
					else
						handler.QueryEditor.TintColor = UIKit.UIColor.Clear;
#endif
				}
			});
		}

	}
}