using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using Microsoft.Extensions.Logging;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public class SwipeItemButton : UIButton
	{
		internal UIImage? OriginalImage { get; set; }

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
		WeakReference<MauiSwipeView>? _swipeView;

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
			{
				_proxy.Disconnect(swipeItemButton);
				swipeItemButton.OriginalImage = null;
			}
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
			InvalidateSwipeItemSize(handler);
		}

		public static void MapFont(ISwipeItemMenuItemHandler handler, ITextStyle view)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(view, fontManager);
			InvalidateSwipeItemSize(handler);
		}

		public static void MapText(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			handler.PlatformView.RestorationIdentifier = view.Text;
			handler.PlatformView.SetTitle(view.Text, UIControlState.Normal);
			InvalidateSwipeItemSize(handler);
		}

		public static void MapBackground(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			handler.PlatformView.UpdateBackground(view.Background);
		}

		public static void MapVisibility(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			var swipeView = handler.PlatformView.GetParentOfType<MauiSwipeView>();

			// Update the native view's Hidden state BEFORE calling UpdateIsVisibleSwipeItem,
			// so LayoutSwipeItems can use the correct Hidden state when repositioning items.
			handler.PlatformView.UpdateVisibility(view.Visibility);

			swipeView?.UpdateIsVisibleSwipeItem(view);
		}

		static void InvalidateSwipeItemSize(ISwipeItemMenuItemHandler? handler)
		{
			if (handler?.PlatformView is not UIView platformView)
				return;

			var swipeView = platformView.GetParentOfType<MauiSwipeView>();
			if (handler is SwipeItemMenuItemHandler platformHandler)
			{
				if (swipeView != null)
					platformHandler.SetSwipeView(swipeView);
				else if (platformHandler._swipeView?.TryGetTarget(out swipeView) != true)
					swipeView = null;
			}

			swipeView?.UpdateSwipeItemSize(handler.VirtualView);
		}

		internal void SetSwipeView(MauiSwipeView swipeView)
		{
			_swipeView = new(swipeView);
		}

		void UpdateImage(UIImage? platformImage, bool sourceChanged)
		{
			if (PlatformView is not SwipeItemButton button || VirtualView is not ISwipeItemMenuItem item)
				return;

			if (sourceChanged)
				button.OriginalImage = platformImage;

			var frame = button.Frame;
			if (frame == CGRect.Empty)
				return;

			if (platformImage == null)
			{
				button.SetImage(null, UIControlState.Normal);

				if (sourceChanged)
					InvalidateSwipeItemSize(this);

				return;
			}

			var maxWidth = frame.Width * 0.5f;
			var maxHeight = frame.Height * 0.5f;
			var resizedImage = MaxResizeSwipeItemIconImage(platformImage, maxWidth, maxHeight);

			try
			{
				button.SetImage(resizedImage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);

				if (item.Source is IFontImageSource fontImageSource && fontImageSource.Color != null)
					button.TintColor = fontImageSource.Color.ToPlatform();
				else
				{
					var tintColor = item.GetTextColor();
					if (tintColor != null)
						button.TintColor = tintColor.ToPlatform();
				}

				if (sourceChanged)
					InvalidateSwipeItemSize(this);
			}
			catch (Exception)
			{
				// UIImage ctor throws on file not found if MonoTouch.ObjCRuntime.Class.ThrowOnInitFailure is true;
				MauiContext?.CreateLogger<SwipeItemMenuItemHandler>()?.LogWarning("Cannot load SwipeItem Icon");
			}
		}

		static UIImage MaxResizeSwipeItemIconImage(UIImage sourceImage, nfloat maxWidth, nfloat maxHeight)
		{
			var sourceSize = sourceImage.Size;
			var maxResizeFactor = Math.Min(maxWidth / sourceSize.Width, maxHeight / sourceSize.Height);

			if (maxResizeFactor > 1)
				return sourceImage;

			var width = maxResizeFactor * sourceSize.Width;
			var height = maxResizeFactor * sourceSize.Height;

			var format = new UIGraphicsImageRendererFormat
			{
				Opaque = false,
				Scale = 0
			};

			using (var renderer = new UIGraphicsImageRenderer(new CGSize(width, height), format))
			{
				return renderer.CreateImage((UIGraphicsImageRendererContext imageContext) =>
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
			}
		}

		partial class SwipeItemMenuItemImageSourcePartSetter
		{
			public override void SetImageSource(UIImage? platformImage)
			{
				if (Handler is SwipeItemMenuItemHandler handler)
					handler.UpdateImage(platformImage, true);
			}
		}

		class SwipeItemButtonProxy
		{
			WeakReference<ISwipeItemMenuItemHandler>? _handler;
			CGSize _lastFrameSize;

			public void Connect(ISwipeItemMenuItemHandler handler, SwipeItemButton platformView)
			{
				_handler = new(handler);
				_lastFrameSize = platformView.Frame.Size;
				platformView.FrameChanged += OnSwipeItemFrameChanged;
			}

			public void Disconnect(SwipeItemButton platformView)
			{
				platformView.FrameChanged -= OnSwipeItemFrameChanged;
				_handler = null;
				_lastFrameSize = CGSize.Empty;
			}

			void OnSwipeItemFrameChanged(object? sender, EventArgs e)
			{
				if (sender is not UIView platformView || _lastFrameSize.Equals(platformView.Frame.Size))
					return;

				_lastFrameSize = platformView.Frame.Size;

				// Adjust the size of the icon in case of changing the size of the SwipeItem.
				if (_handler is not null &&
					_handler.TryGetTarget(out var swipeItemMenuItemHandler) &&
					swipeItemMenuItemHandler is SwipeItemMenuItemHandler handler &&
					handler.PlatformView is SwipeItemButton button)
				{
					handler.UpdateImage(button.OriginalImage, false);
				}
			}
		}
	}
}
