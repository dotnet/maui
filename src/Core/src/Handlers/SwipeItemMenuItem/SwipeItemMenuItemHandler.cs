#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIButton;
#elif MONOANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.SwipeItem;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.NUI.Button;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

using System.Threading.Tasks;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeItemMenuItemHandler : ISwipeItemMenuItemHandler
	{
		public static IPropertyMapper<ISwipeItemMenuItem, ISwipeItemMenuItemHandler> Mapper =
			new PropertyMapper<ISwipeItemMenuItem, ISwipeItemMenuItemHandler>(ViewHandler.ElementMapper)
			{
				[nameof(ISwipeItemMenuItem.Visibility)] = MapVisibility,
				[nameof(IView.Background)] = MapBackground,
				[nameof(IMenuElement.Text)] = MapText,
				[nameof(ITextStyle.TextColor)] = MapTextColor,
				[nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
				[nameof(ITextStyle.Font)] = MapFont,
				[nameof(IMenuElement.Source)] = MapSource,
				[nameof(ISwipeItemMenuItemIconColor.IconColor)] = MapIconColor,
			};

		public static CommandMapper<ISwipeItemMenuItem, ISwipeItemMenuItemHandler> CommandMapper =
			new(ElementHandler.ElementCommandMapper)
			{
			};


		public SwipeItemMenuItemHandler() : base(Mapper, CommandMapper)
		{

		}

		protected SwipeItemMenuItemHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		protected SwipeItemMenuItemHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		ISwipeItemMenuItem ISwipeItemMenuItemHandler.VirtualView => VirtualView;

		PlatformView ISwipeItemMenuItemHandler.PlatformView => PlatformView;

		ImageSourcePartLoader? _imageSourcePartLoader;

		public virtual ImageSourcePartLoader SourceLoader =>
			_imageSourcePartLoader ??= new ImageSourcePartLoader(new SwipeItemMenuItemImageSourcePartSetter(this));

		public static void MapSource(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem image)
		{
			MapSourceAsync(handler, image).FireAndForget(handler);
		}

		public static void MapIconColor(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			if (handler.IsMappingProperties())
				return;

			UpdateIconColor(handler, view);
		}

		internal static void UpdateBackgroundColorDependencies(ISwipeItemMenuItemHandler handler)
		{
			if (handler.IsMappingProperties())
				return;

			handler.UpdateValue(nameof(ITextStyle.TextColor));
		}

		internal static void UpdateTextColorIconDependency(
			ISwipeItemMenuItemHandler handler,
			ISwipeItemMenuItem view)
		{
			if (!handler.IsMappingProperties() &&
				view is not ISwipeItemMenuItemIconColor { IconColor: not null } &&
				view.Source is IFontImageSource { Color: null })
			{
				handler.UpdateValue(nameof(ISwipeItemMenuItemIconColor.IconColor));
			}
		}

		static void UpdateIconColor(
			ISwipeItemMenuItemHandler handler,
			ISwipeItemMenuItem view)
		{
			bool handled = false;
			UpdateIconColorPlatform(handler, view, ref handled);

			// Platforms that cannot safely reapply the attached native image must reload the
			// source so the platform image type or rendered font color can be recomputed.
			if (!handled)
				handler.UpdateValue(nameof(IMenuElement.Source));
		}

		static partial void UpdateIconColorPlatform(
			ISwipeItemMenuItemHandler handler,
			ISwipeItemMenuItem view,
			ref bool handled);

		public static Task MapSourceAsync(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem image)
		{
#if WINDOWS
			return LoadIconAsync(handler, image);
#else
			if (handler.SourceLoader is ImageSourcePartLoader loader)
				return loader.UpdateImageSourceAsync();

			return Task.CompletedTask;
#endif
		}

		partial class SwipeItemMenuItemImageSourcePartSetter : ImageSourcePartSetter<ISwipeItemMenuItemHandler>
		{
			public SwipeItemMenuItemImageSourcePartSetter(ISwipeItemMenuItemHandler handler)
				: base(handler)
			{
			}
		}

	}
}
