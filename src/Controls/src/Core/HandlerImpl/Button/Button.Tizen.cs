using Microsoft.Maui.Controls.Platform;
using Tizen.NUI;

namespace Microsoft.Maui.Controls
{
	public partial class Button
	{
		public static void MapText(IButtonHandler handler, Button button)
		{
			handler.PlatformView?.UpdateText(button);
		}

		public static void MapText(ButtonHandler handler, Button button) =>
			MapText((IButtonHandler)handler, button);

		public static void MapLineBreakMode(IButtonHandler handler, Button button)
		{
			switch (button.LineBreakMode)
			{
				case LineBreakMode.NoWrap:
					handler.PlatformView.TextLabel.MultiLine = false;
					handler.PlatformView.TextLabel.Ellipsis = false;
					break;
				case LineBreakMode.WordWrap:
					handler.PlatformView.TextLabel.MultiLine = true;
					handler.PlatformView.TextLabel.Ellipsis = false;
					handler.PlatformView.TextLabel.LineWrapMode = LineWrapMode.Word;
					break;
				case LineBreakMode.CharacterWrap:
					handler.PlatformView.TextLabel.MultiLine = true;
					handler.PlatformView.TextLabel.Ellipsis = false;
					handler.PlatformView.TextLabel.LineWrapMode = LineWrapMode.Character;
					break;
				case LineBreakMode.HeadTruncation:
				case LineBreakMode.TailTruncation:
				case LineBreakMode.MiddleTruncation:
					handler.PlatformView.TextLabel.MultiLine = false;
					handler.PlatformView.TextLabel.Ellipsis = true;
					break;
			}
		}
	}
}
