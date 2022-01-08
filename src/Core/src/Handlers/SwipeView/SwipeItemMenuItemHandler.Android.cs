using Android.Views;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;
using System;
using AButton = AndroidX.AppCompat.Widget.AppCompatButton;
using ATextAlignment = Android.Views.TextAlignment;
using Android.Graphics.Drawables;
using Android.Widget;
using System.Threading.Tasks;

namespace Microsoft.Maui.Handlers
{
	public class SwipeItemMenuItemHandler : ElementHandler<ISwipeItemMenuItem, AView>
	{
		//TODO MOVE ELSEWHERE
		const int SwipeThreshold = 250;
		const int SwipeItemWidth = 100;

		public static IPropertyMapper<ISwipeItemMenuItem, SwipeItemMenuItemHandler> Mapper = new PropertyMapper<ISwipeItemMenuItem, SwipeItemMenuItemHandler>(ViewHandler.ViewMapper)
		{
			[nameof(ISwipeItemMenuItem.IsVisible)] = MapVisible,
			[nameof(IView.Background)] = MapBackground,
			[nameof(IMenuElement.Text)] = MapText,
			[nameof(IMenuElement.Source)] = MapSource,

		};

		public static CommandMapper<ISwipeItemMenuItem, ISwipeViewHandler> CommandMapper = new(ViewHandler.ViewCommandMapper)
		{
		};


		ImageSourcePartLoader? _imageSourcePartLoader;
		public ImageSourcePartLoader SourceLoader =>
			_imageSourcePartLoader ??= new ImageSourcePartLoader(this, () => VirtualView, OnSetImageSource);


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

		protected override void ConnectHandler(AView nativeView)
		{
			base.ConnectHandler(nativeView);
			NativeView.ViewAttachedToWindow += OnViewAttachedToWindow;
		}

		void OnViewAttachedToWindow(object? sender, AView.ViewAttachedToWindowEventArgs e)
		{
			UpdateSize();
		}

		protected override void DisconnectHandler(AView nativeView)
		{
			base.DisconnectHandler(nativeView);
			nativeView.ViewAttachedToWindow -= OnViewAttachedToWindow;
		}

		public static void MapSource(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem image) =>
			MapSourceAsync(handler, image).FireAndForget(handler);

		public static Task MapSourceAsync(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem image)
		{
			return handler.SourceLoader.UpdateImageSourceAsync();
		}

		public static void MapText(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			// TODO ITextStyle
			if (handler.NativeView is TextView text)
				text.Text = handler.VirtualView.Text ?? string.Empty;

			handler.UpdateSize();
		}

		public static void MapBackground(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			handler.NativeView.UpdateBackground(handler.VirtualView.Background);

			var textColor = GetSwipeItemColor(handler.VirtualView.Background?.ToColor());

			if (handler.NativeView is TextView textView)
			{
				if (textColor != null)
					textView.SetTextColor(textColor.ToNative());

				textView.TextAlignment = ATextAlignment.Center;
			}
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

			var swipeButton = new AButton(MauiContext.Context);
			swipeButton.SetOnTouchListener(null);
			swipeButton.Visibility = VirtualView.IsVisible ? ViewStates.Visible : ViewStates.Gone;

			// TODO Fix AutomationID probably with separate interface
			//if (!string.IsNullOrEmpty(formsSwipeItem.AutomationId))
			//	swipeButton.ContentDescription = formsSwipeItem.AutomationId;

			return swipeButton;
		}

		int GetIconSize()
		{
			if (VirtualView is not IImageSourcePart imageSourcePart || imageSourcePart.Source == null)
				return 0;

			var mauiSwipeView = NativeView.Parent.GetParentOfType<MauiSwipeView>();

			if (mauiSwipeView == null || MauiContext?.Context == null)
				return 0;

			int contentHeight = mauiSwipeView.Height;
			int contentWidth = (int)MauiContext.Context.ToPixels(SwipeItemWidth);

			return Math.Min(contentHeight, contentWidth) / 2;
		}

		void UpdateSize()
		{
			var textSize = 0;
			var contentHeight = 0;


			var mauiSwipeView = NativeView.Parent.GetParentOfType<MauiSwipeView>();
			if (mauiSwipeView != null)
				contentHeight = mauiSwipeView.Height;

			if (NativeView is TextView textView)
				textSize = !string.IsNullOrEmpty(textView.Text) ? (int)textView.TextSize : 0;

			var iconSize = GetIconSize();
			var buttonPadding = (contentHeight - (iconSize + textSize + 6)) / 2;
			NativeView.SetPadding(0, buttonPadding, 0, buttonPadding);
		}

		void OnSetImageSource(Drawable? drawable)
		{
			if (drawable != null)
			{
				var iconSize = GetIconSize();
				var textColor = GetSwipeItemColor(VirtualView.Background?.ToColor());
				int drawableWidth = drawable.IntrinsicWidth;
				int drawableHeight = drawable.IntrinsicHeight;

				if (drawableWidth > drawableHeight)
				{
					var iconWidth = iconSize;
					var iconHeight = drawableHeight * iconWidth / drawableWidth;
					drawable.SetBounds(0, 0, iconWidth, iconHeight);
				}
				else
				{
					var iconHeight = iconSize;
					var iconWidth = drawableWidth * iconHeight / drawableHeight;
					drawable.SetBounds(0, 0, iconWidth, iconHeight);
				}

				if (textColor != null)
					drawable.SetColorFilter(textColor.ToNative(), FilterMode.SrcAtop);
			}

			(NativeView as TextView)?.SetCompoundDrawables(null, drawable, null, null);
			UpdateSize();
		}
	}
}
