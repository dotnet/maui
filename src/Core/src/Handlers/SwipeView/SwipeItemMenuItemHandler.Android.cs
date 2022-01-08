using Android.Views;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;
using System;
using AButton = AndroidX.AppCompat.Widget.AppCompatButton;
using ATextAlignment = Android.Views.TextAlignment;

namespace Microsoft.Maui.Handlers
{
	public class SwipeItemMenuItemHandler : ElementHandler<ISwipeItemMenuItem, AView>
	{
		public static IPropertyMapper<ISwipeItemMenuItem, SwipeItemMenuItemHandler> Mapper = new PropertyMapper<ISwipeItemMenuItem, SwipeItemMenuItemHandler>(ViewHandler.ViewMapper)
		{
			[nameof(ISwipeItemMenuItem.IsVisible)] = MapVisible
		};

		public static CommandMapper<ISwipeItemMenuItem, ISwipeViewHandler> CommandMapper = new(ViewHandler.ViewCommandMapper)
		{
		};

		public SwipeItemMenuItemHandler() : base(Mapper, CommandMapper)
		{

		}

		protected SwipeItemMenuItemHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper ?? CommandMapper)
		{
		}

		public SwipeItemMenuItemHandler(IPropertyMapper? mapper = null) : base(mapper ?? Mapper)
		{

		}

		public static void MapVisible(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			var swipeView = handler.NativeView?.Parent.GetParentOfType<MauiSwipeView>();
			if (swipeView != null)
				swipeView.UpdateIsVisibleSwipeItem(view);
		}

		static Color? GetSwipeItemColor(Color? backgroundColor)
		{
			if (backgroundColor == null)
				return null;

			var luminosity = 0.2126f * backgroundColor.Red + 0.7152f * backgroundColor.Green + 0.0722f * backgroundColor.Blue;

			return luminosity < 0.75f ? Colors.White : Colors.Black;
		}

		protected override AView CreateNativeElement()
		{
			_ = MauiContext?.Context ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var swipeButton = new AButton(MauiContext.Context)
			{
				Text = VirtualView.Text ?? string.Empty
			};

			swipeButton.UpdateBackground(VirtualView.Background);

			if (!string.IsNullOrEmpty(VirtualView.AutomationId))
				swipeButton.ContentDescription = VirtualView.AutomationId;

			var textColor = GetSwipeItemColor(VirtualView.Background?.ToColor());

			if (textColor != null)
				swipeButton.SetTextColor(textColor.ToNative());

			swipeButton.TextAlignment = ATextAlignment.Center;

			//int contentHeight = _contentView.Height;
			//int contentWidth = (int)MauiContext.Context.ToPixels(SwipeItemWidth);

			//int iconSize = 0;
			//int iconSize = formsSwipeItem.IconImageSource != null ? Math.Min(contentHeight, contentWidth) / 2 : 0;

			//_ = this.ApplyDrawableAsync(formsSwipeItem, MenuItem.IconImageSourceProperty, Context, drawable =>
			//{
			//	if (drawable != null)
			//	{
			//		int drawableWidth = drawable.IntrinsicWidth;
			//		int drawableHeight = drawable.IntrinsicHeight;

			//		if (drawableWidth > drawableHeight)
			//		{
			//			var iconWidth = iconSize;
			//			var iconHeight = drawableHeight * iconWidth / drawableWidth;
			//			drawable.SetBounds(0, 0, iconWidth, iconHeight);
			//		}
			//		else
			//		{
			//			var iconHeight = iconSize;
			//			var iconWidth = drawableWidth * iconHeight / drawableHeight;
			//			drawable.SetBounds(0, 0, iconWidth, iconHeight);
			//		}

			//		if (textColor != null)
			//			drawable.SetColorFilter(textColor.ToNative(), FilterMode.SrcAtop);
			//	}

			//	swipeButton.SetCompoundDrawables(null, drawable, null, null);
			//});

			var textSize = !string.IsNullOrEmpty(swipeButton.Text) ? (int)swipeButton.TextSize : 0;
			//var buttonPadding = (contentHeight - (iconSize + textSize + 6)) / 2;
			//swipeButton.SetPadding(0, buttonPadding, 0, buttonPadding);
			swipeButton.SetOnTouchListener(null);
			swipeButton.Visibility = VirtualView.IsVisible ? ViewStates.Visible : ViewStates.Gone;

			// TODO Fix AutomationID probably with separate interface
			//if (!string.IsNullOrEmpty(formsSwipeItem.AutomationId))
			//	swipeButton.ContentDescription = formsSwipeItem.AutomationId;

			return swipeButton;
		}
	}
}
