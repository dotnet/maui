using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Widget;
using AButton = AndroidX.AppCompat.Widget.AppCompatButton;
using ATextAlignment = Android.Views.TextAlignment;
using AView = Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeItemMenuItemHandler : ElementHandler<ISwipeItemMenuItem, AView>
	{
		protected override void ConnectHandler(AView platformView)
		{
			base.ConnectHandler(platformView);
			platformView.ViewAttachedToWindow += OnViewAttachedToWindow;
		}

		void OnViewAttachedToWindow(object? sender, AView.ViewAttachedToWindowEventArgs e)
		{
			UpdateSize();
		}

		protected override void DisconnectHandler(AView platformView)
		{
			base.DisconnectHandler(platformView);
			platformView.ViewAttachedToWindow -= OnViewAttachedToWindow;
		}

		public static void MapTextColor(ISwipeItemMenuItemHandler handler, ITextStyle view)
		{
			(handler.PlatformView as TextView)?.UpdateTextColor(view);
		}

		public static void MapCharacterSpacing(ISwipeItemMenuItemHandler handler, ITextStyle view)
		{
			(handler.PlatformView as TextView)?.UpdateCharacterSpacing(view);
		}

		public static void MapFont(ISwipeItemMenuItemHandler handler, ITextStyle view)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			(handler.PlatformView as TextView)?.UpdateFont(view, fontManager);
		}

		public static void MapText(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{

			(handler.PlatformView as TextView)?.UpdateTextPlainText(view);

			if (handler is SwipeItemMenuItemHandler platformHandler)
				platformHandler.UpdateSize();
		}

		public static void MapBackground(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			handler.PlatformView.UpdateBackground(handler.VirtualView.Background);

			var textColor = handler.VirtualView.GetTextColor()?.ToPlatform();

			if (handler.PlatformView is TextView textView)
			{
				if (textColor != null)
					textView.SetTextColor(textColor.Value);

				textView.TextAlignment = ATextAlignment.Center;
			}
		}

		public static void MapVisibility(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			var swipeView = handler.PlatformView.Parent.GetParentOfType<MauiSwipeView>();
			swipeView?.UpdateIsVisibleSwipeItem(view);

			handler.PlatformView.Visibility = view.Visibility.ToPlatformVisibility();
		}

		protected override AView CreatePlatformElement()
		{
			_ = MauiContext?.Context ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var swipeButton = new AButton(MauiContext.Context);
			swipeButton.SetOnTouchListener(null);

			if (!string.IsNullOrEmpty(VirtualView.AutomationId))
				swipeButton.ContentDescription = VirtualView.AutomationId;

			return swipeButton;
		}

		static int GetIconSize(ISwipeItemMenuItemHandler handler)
		{
			if (handler.VirtualView is not IImageSourcePart imageSourcePart || imageSourcePart.Source is null)
				return 0;

			var mauiSwipeView = handler.PlatformView.Parent.GetParentOfType<MauiSwipeView>();

			if (mauiSwipeView is null || handler.MauiContext?.Context is null)
				return 0;

			int contentHeight = mauiSwipeView.MeasuredHeight;
			int contentWidth = (int)handler.MauiContext.Context.ToPixels(SwipeViewExtensions.SwipeItemWidth);

			return Math.Min(contentHeight, contentWidth) / 2;
		}

		void UpdateSize()
		{
			var mauiSwipeView = PlatformView.Parent.GetParentOfType<MauiSwipeView>();

			if (mauiSwipeView == null)
				return;

			var contentHeight = mauiSwipeView.MeasuredHeight;

			var swipeView = VirtualView?.FindParentOfType<ISwipeView>();
			float density = mauiSwipeView.Context.GetDisplayDensity();

			if (swipeView?.Content is IView content)
			{
				var verticalThickness = (int)(content.Margin.VerticalThickness * density);
				contentHeight -= verticalThickness;
			}

			var lineHeight = 0;

			if (PlatformView is TextView textView)
			{
				lineHeight = !string.IsNullOrEmpty(textView.Text) ? (int)textView.LineHeight : 0;
				var icons = textView.GetCompoundDrawables();
				if (icons.Length > 1 && icons[1] != null)
				{
					SourceLoader.Setter.SetImageSource(icons[1]);
				}
			}

			var iconSize = GetIconSize(this);
			var textPadding = 2 * density;
			var buttonPadding = (int)((contentHeight - (iconSize + lineHeight + textPadding)) / 2);
			PlatformView.SetPadding(0, buttonPadding, 0, buttonPadding);
		}

		partial class SwipeItemMenuItemImageSourcePartSetter
		{
			public override void SetImageSource(Drawable? platformImage)
			{
				if (Handler?.PlatformView is not TextView button || Handler?.VirtualView is not ISwipeItemMenuItem item)
					return;

				if (platformImage is not null)
				{
					var iconSize = GetIconSize(Handler);
					var textColor = item.GetTextColor()?.ToPlatform();
					int drawableWidth = platformImage.IntrinsicWidth;
					int drawableHeight = platformImage.IntrinsicHeight;

					if (drawableWidth > drawableHeight)
					{
						var iconWidth = iconSize;
						var iconHeight = drawableHeight * iconWidth / drawableWidth;
						platformImage.SetBounds(0, 0, iconWidth, iconHeight);
					}
					else
					{
						var iconHeight = iconSize;
						var iconWidth = drawableWidth * iconHeight / drawableHeight;
						platformImage.SetBounds(0, 0, iconWidth, iconHeight);
					}

					if (textColor != null)
						platformImage.SetColorFilter(textColor.Value, FilterMode.SrcAtop);
				}

				button.SetCompoundDrawables(null, platformImage, null, null);
			}
		}
	}
}