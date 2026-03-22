using Tizen.UIExtensions.NUI;
using TColor = Tizen.UIExtensions.Common.Color;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeItemMenuItemHandler : ElementHandler<ISwipeItemMenuItem, Button>
	{
		protected override Button CreatePlatformElement() => new Button()
		{
			BackgroundColor = Tizen.NUI.Color.Transparent,
			IconRelativeOrientation = Tizen.NUI.Components.Button.IconOrientation.Top,
			CornerRadius = 0,
		};

		public static void MapTextColor(ISwipeItemMenuItemHandler handler, ITextStyle view)
		{
			(handler.PlatformView)?.UpdateTextColor(view);
		}

		[MissingMapper]
		public static void MapCharacterSpacing(ISwipeItemMenuItemHandler handler, ITextStyle view) { }

		[MissingMapper]
		public static void MapFont(ISwipeItemMenuItemHandler handler, ITextStyle view) { }

		public static void MapText(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			handler.PlatformView?.UpdateText(view);
		}

		public static void MapBackground(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			if (handler.PlatformView == null)
				return;

			handler.PlatformView.UpdateBackground(handler.VirtualView.Background);
			var textColor = handler.VirtualView.GetTextColor()?.ToPlatform() ?? TColor.Default;
			if (textColor != TColor.Default)
			{
				handler.PlatformView.TextColor = textColor;
			}
		}

		public static void MapVisibility(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			if (view.Visibility.ToPlatformVisibility())
			{
				handler.PlatformView.Show();
			}
			else
			{
				handler.PlatformView.Hide();
			}

			var swipeView = handler.PlatformView.GetParentOfType<MauiSwipeView>();
			swipeView?.UpdateIsVisibleSwipeItem(view);
		}

		partial class SwipeItemMenuItemImageSourcePartSetter
		{
			public override void SetImageSource(MauiImageSource? platformImage)
			{
				if (Handler?.PlatformView is not Button item)
					return;

				if (platformImage is null)
					return;

				item.Icon.ResourceUrl = platformImage.ResourceUrl;
			}
		}
	}
}
