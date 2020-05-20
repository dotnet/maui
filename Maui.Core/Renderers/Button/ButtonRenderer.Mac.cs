using AppKit;

namespace System.Maui.Platform
{
	public partial class ButtonRenderer
	{
		protected override NSButton CreateView()
		{
			var button = new NSButton();
			return button;
		}

		protected override void SetupDefaults()
		{
			if(TypedNativeView.ContentTintColor != null)
				DefaultTextColor = TypedNativeView.ContentTintColor.ToColor();
		}

		//public static void MapPropertyButtonFont(IViewRenderer renderer, IButton view)
		//{

		//}
		//public static void MapPropertyButtonInputTransparent(IViewRenderer renderer, IButton view)
		//{

		//}

		//public static void MapPropertyButtonCharacterSpacing(IViewRenderer renderer, IButton view)
		//{

		//}
	}
}
