using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using Microsoft.Extensions.Logging;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public class SwipeItemButton : UIButton
	{
		[UnconditionalSuppressMessage("Memory", "MEM0001", Justification = "Proven safe in test: SwipeViewTests.ItemsDoNotLeak")]
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
		readonly SwipeItemButtonProxy _proxy = new();

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
				_proxy.Connect(this, swipeItemButton);
		}

		protected override void DisconnectHandler(UIButton platformView)
		{
			base.DisconnectHandler(platformView);

			if (platformView is SwipeItemButton swipeItemButton)
				_proxy.Disconnect(swipeItemButton);
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

			swipeView?.UpdateIsVisibleSwipeItem(view);

			handler.PlatformView.UpdateVisibility(view.Visibility);
		}

		partial class SwipeItemMenuItemImageSourcePartSetter
		{
			public override void SetImageSource(UIImage? platformImage)
			{
				if (Handler?.PlatformView is not UIButton button || Handler?.VirtualView is not ISwipeItemMenuItem item)
					return;

				var frame = button.Frame;
				if (frame == CGRect.Empty)
					return;

				if (platformImage == null)
				{
					button.SetImage(null, UIControlState.Normal);
				}
				else
				{
					var maxWidth = frame.Width * 0.5f;
					var maxHeight = frame.Height * 0.5f;

					var resizedImage = MaxResizeSwipeItemIconImage(platformImage, maxWidth, maxHeight);

					try
					{
						button.SetImage(resizedImage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
						var tintColor = item.GetTextColor();

						if (tintColor != null)
							button.TintColor = tintColor.ToPlatform();
					}
					catch (Exception)
					{
						// UIImage ctor throws on file not found if MonoTouch.ObjCRuntime.Class.ThrowOnInitFailure is true;
						Handler.MauiContext?.CreateLogger<SwipeItemMenuItemHandler>()?.LogWarning("Cannot load SwipeItem Icon");
					}
				}
			}

			static UIImage MaxResizeSwipeItemIconImage(UIImage sourceImage, nfloat maxWidth, nfloat maxHeight)
			{
				var sourceSize = sourceImage.Size;
				var maxResizeFactor = Math.Min(maxWidth / sourceSize.Width, maxHeight / sourceSize.Height);

				if (maxResizeFactor > 1)
				{
					return sourceImage;
				}

				var width = maxResizeFactor * sourceSize.Width;
				var height = maxResizeFactor * sourceSize.Height;

				var format = new UIGraphicsImageRendererFormat
				{
					Opaque = false,
					Scale = 0
				};

				using (var renderer = new UIGraphicsImageRenderer(new CGSize(width, height), format))
				{
					var resultImage = renderer.CreateImage((UIGraphicsImageRendererContext imageContext) =>
					{
						var cgcontext = imageContext.CGContext;

						// The image is drawn upside down because Core Graphics uses a bottom-left origin, 
						// whereas UIKit uses a top-left origin. Adjust the coordinate system to align with UIKit's top-left origin.
						cgcontext.TranslateCTM(0, (nfloat)height);
						cgcontext.ScaleCTM(1, -1);
						cgcontext.DrawImage(new CGRect(0, 0, (nfloat)width, (nfloat)height), sourceImage.CGImage);
						cgcontext.ScaleCTM(1, -1);
						cgcontext.TranslateCTM(0, -(nfloat)height);
					});

					return resultImage;
				}
			}
		}

		class SwipeItemButtonProxy
		{
			WeakReference<ISwipeItemMenuItemHandler>? _handler;

			public void Connect(ISwipeItemMenuItemHandler handler, SwipeItemButton platformView)
			{
				_handler = new(handler);
				platformView.FrameChanged += OnSwipeItemFrameChanged;
			}

			public void Disconnect(SwipeItemButton platformView)
			{
				platformView.FrameChanged -= OnSwipeItemFrameChanged;
			}

			void OnSwipeItemFrameChanged(object? sender, EventArgs e)
			{
				// Adjust the size of the icon in case of changing the size of the SwipeItem.
				if (_handler is not null && _handler.TryGetTarget(out var swipeItemMenuItemHandler))
					swipeItemMenuItemHandler.UpdateValue(nameof(ISwipeItemMenuItem.Source));
			}
		}
	}
}
