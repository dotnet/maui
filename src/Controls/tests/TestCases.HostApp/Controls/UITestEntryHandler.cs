using Microsoft.Maui.Handlers;

namespace Maui.Controls.Sample
{
	public class UITestEntryHandler : EntryHandler
	{
		public UITestEntryHandler() : base(EntryHandler.Mapper)
		{
			Mapper.AppendToMapping(nameof(IUITestEntry.IsCursorVisible), (handler, entry) =>
			{
				if (entry is UITestEntry testEntry)
				{
					bool isCursorVisible = testEntry.IsCursorVisible;
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