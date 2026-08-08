using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Widget;
using Microsoft.Maui.Graphics;
using AButton = AndroidX.AppCompat.Widget.AppCompatButton;
using ATextAlignment = Android.Views.TextAlignment;
using AView = Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeItemMenuItemHandler : ElementHandler<ISwipeItemMenuItem, AView>
	{
		const int IconDrawableSlot = 1;

		Drawable? _appliedIconTintDrawable;

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
			platformView.ViewAttachedToWindow -= OnViewAttachedToWindow;
			_appliedIconTintDrawable = null;
			base.DisconnectHandler(platformView);
		}

		public static void MapTextColor(ISwipeItemMenuItemHandler handler, ITextStyle view)
		{
			if (handler.PlatformView is not TextView textView)
				return;

			// The mapper is PropertyMapper<ISwipeItemMenuItem, ...> so view is always an
			// ISwipeItemMenuItem at runtime; route through GetTextColor() to include the
			// luminosity-contrast fallback.
			Color? resolved = view is ISwipeItemMenuItem swipeItem ? swipeItem.GetTextColor() : view.TextColor;

			// UpdateTextColor restores the cached themed default when resolved is null so a
			// previously applied color is not left stale (for example when Background is removed).
			textView.UpdateTextColor(resolved);

			if (view is ISwipeItemMenuItem swipeItemMenuItem)
				UpdateTextColorIconDependency(handler, swipeItemMenuItem);
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
			handler.PlatformView.UpdateBackground(view.Background);

			if (handler.PlatformView is TextView textView)
				textView.TextAlignment = ATextAlignment.Center;

			UpdateBackgroundColorDependencies(handler);
		}

		public static void MapVisibility(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			// Set visibility before UpdateIsVisibleSwipeItem so LayoutSwipeItems
			// reads the correct visibility when recalculating item positions.
			handler.PlatformView.Visibility = view.Visibility.ToPlatformVisibility();

			var swipeView = handler.PlatformView.Parent.GetParentOfType<MauiSwipeView>();
			swipeView?.UpdateIsVisibleSwipeItem(view);
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
			int maxIconSize = Math.Min(contentHeight, contentWidth) / 2;

			if (imageSourcePart.Source is IFontImageSource fontImageSource)
			{
				var fontManager = handler.GetRequiredService<IFontManager>();
				var fontSize = fontManager.GetFontSize(fontImageSource.Font);
				var requestedIconSize = (int)TypedValue.ApplyDimension(
					fontSize.Unit,
					fontSize.Value,
					handler.MauiContext.Context.Resources?.DisplayMetrics);

				return Math.Min(requestedIconSize, maxIconSize);
			}

			return maxIconSize;
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
				if (GetIconDrawable(textView) is Drawable icon)
				{
					SourceLoader.Setter.SetImageSource(icon);
				}
			}

			var iconSize = GetIconSize(this);
			var textPadding = 2 * density;
			var buttonPadding = (int)((contentHeight - (iconSize + lineHeight + textPadding)) / 2);
			PlatformView.SetPadding(0, buttonPadding, 0, buttonPadding);
		}

		static partial void UpdateIconColorPlatform(
			ISwipeItemMenuItemHandler handler,
			ISwipeItemMenuItem view,
			ref bool handled)
		{
			if (handler.PlatformView is not TextView button ||
				handler.SourceLoader is not ImageSourcePartLoader loader)
			{
				return;
			}

			if (GetIconDrawable(button) is not Drawable current)
				return;

			loader.Setter.SetImageSource(current);
			handled = true;
		}

		static Drawable? GetIconDrawable(TextView textView)
		{
			var drawables = textView.GetCompoundDrawables();
			return drawables.Length > IconDrawableSlot ? drawables[IconDrawableSlot] : null;
		}

		static void SetIconDrawable(TextView textView, Drawable? drawable)
		{
			textView.SetCompoundDrawables(null, drawable, null, null);
		}

		partial class SwipeItemMenuItemImageSourcePartSetter
		{
			public override void SetImageSource(Drawable? platformImage)
			{
				if (Handler is not SwipeItemMenuItemHandler platformHandler ||
					Handler.PlatformView is not TextView button ||
					Handler.VirtualView is not ISwipeItemMenuItem item)
					return;

				var tintColor = item.GetIconTintColor()?.ToPlatform();

				if (platformImage is not null)
				{
					var iconSize = GetIconSize(Handler);
					// Drawable.ColorFilter is not authoritative because the base Android
					// Drawable implementation always returns null.
					bool clearTint = tintColor is null &&
						ReferenceEquals(platformImage, platformHandler._appliedIconTintDrawable);

					if (tintColor is not null || clearTint)
					{
						// File/resource image services can return drawables backed by a shared
						// ConstantState. Mutate only before changing its color filter.
						platformImage = platformImage.Mutate();
					}

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

					if (tintColor is not null)
					{
						platformImage.SetColorFilter(tintColor.Value, FilterMode.SrcAtop);
					}
					else if (clearTint)
					{
						platformImage.ClearColorFilter();
					}
				}

				platformHandler._appliedIconTintDrawable =
					tintColor is not null ? platformImage : null;
				SetIconDrawable(button, platformImage);
			}
		}
	}
}
