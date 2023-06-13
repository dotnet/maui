using System;
using CoreGraphics;
using Microsoft.Extensions.Logging;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public class SwipeItemButton : UIButton
	{
		public event EventHandler? FrameChanged;

		public override CGRect Frame
		{
			get => base.Frame;
			set
			{
				base.Frame = value;
				FrameChanged?.Invoke(this, EventArgs.Empty);
			}
		}
	}

	public partial class SwipeItemMenuItemHandler : ElementHandler<ISwipeItemMenuItem, UIButton>
	{
		protected override UIButton CreatePlatformElement()
		{
			var swipeItemButton = new SwipeItemButton
			{
				RestorationIdentifier = VirtualView.Text,
				UserInteractionEnabled = false
			};

			return swipeItemButton;
		}

		protected override void ConnectHandler(UIButton platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView is SwipeItemButton swipeItemButton)
				swipeItemButton.FrameChanged += OnSwipeItemFrameChanged;
		}

		protected override void DisconnectHandler(UIButton platformView)
		{
			base.DisconnectHandler(platformView);

			if (platformView is SwipeItemButton swipeItemButton)
				swipeItemButton.FrameChanged -= OnSwipeItemFrameChanged;
		}

		public static void MapTextColor(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			var color = view.GetTextColor();

			if (color != null)
				handler.PlatformView.SetTitleColor(color.ToPlatform(), UIControlState.Normal);
		}

		public static void MapCharacterSpacing(ISwipeItemMenuItemHandler handler, ITextStyle view)
		{
			handler.PlatformView?.UpdateCharacterSpacing(view);
		}

		public static void MapFont(ISwipeItemMenuItemHandler handler, ITextStyle view)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(view, fontManager);
		}

		public static void MapText(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			handler.PlatformView.RestorationIdentifier = view.Text;
			handler.PlatformView.SetTitle(view.Text, UIControlState.Normal);
		}

		public static void MapBackground(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			handler.PlatformView.UpdateBackground(view.Background);
		}

		public static void MapVisibility(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			var swipeView = handler.PlatformView.GetParentOfType<MauiSwipeView>();

			if (swipeView != null)
				swipeView.UpdateIsVisibleSwipeItem(view);

			handler.PlatformView.UpdateVisibility(view.Visibility);
		}

		void OnSwipeItemFrameChanged(object? sender, EventArgs e)
		{
			// Adjust the size of the icon in case of changing the size of the SwipeItem.
			if (this is ISwipeItemMenuItemHandler swipeItemMenuItemHandler)
				swipeItemMenuItemHandler.UpdateValue(nameof(ISwipeItemMenuItem.Source));
		}

		void OnSetImageSource(UIImage? image)
		{
			if (PlatformView == null || PlatformView.Frame == CGRect.Empty)
				return;

			if (image == null)
				PlatformView.SetImage(null, UIControlState.Normal);
			else
			{
				var maxWidth = PlatformView.Frame.Width * 0.5f;
				var maxHeight = PlatformView.Frame.Height * 0.5f;

				var resizedImage = MaxResizeSwipeItemIconImage(image, maxWidth, maxHeight);

				try
				{
					PlatformView.SetImage(resizedImage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
					var tintColor = VirtualView.GetTextColor();

					if (tintColor != null)
						PlatformView.TintColor = tintColor.ToPlatform();
				}
				catch (Exception)
				{
					// UIImage ctor throws on file not found if MonoTouch.ObjCRuntime.Class.ThrowOnInitFailure is true;
					MauiContext?.CreateLogger<SwipeItemMenuItemHandler>()?.LogWarning("Can not load SwipeItem Icon");
				}
			}
		}

		UIImage MaxResizeSwipeItemIconImage(UIImage sourceImage, nfloat maxWidth, nfloat maxHeight)
		{
			var sourceSize = sourceImage.Size;
			var maxResizeFactor = Math.Min(maxWidth / sourceSize.Width, maxHeight / sourceSize.Height);

			if (maxResizeFactor > 1)
				return sourceImage;

			var width = maxResizeFactor * sourceSize.Width;
			var height = maxResizeFactor * sourceSize.Height;
			UIGraphics.BeginImageContextWithOptions(new CGSize((nfloat)width, (nfloat)height), false, 0);
			sourceImage.Draw(new CGRect(0, 0, (nfloat)width, (nfloat)height));
			var resultImage = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			return resultImage;
		}
	}
}
