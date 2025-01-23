using Microsoft.Maui.Handlers;

namespace Maui.Controls.Sample
{
	public class UITestEntryHandler : EntryHandler
	{
		public UITestEntryHandler() : base(EntryHandler.Mapper)
		{
			Mapper.AppendToMapping("HideCursor", (handler, entry) =>
			{
#if ANDROID
				handler.PlatformView.SetCursorVisible(false);
#elif IOS || MACCATALYST
				handler.PlatformView.TintColor = UIKit.UIColor.Clear;
#endif
			});
		}

	}
}