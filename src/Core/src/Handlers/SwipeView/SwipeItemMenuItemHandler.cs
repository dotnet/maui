using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeItemMenuItemHandler
	{
		public static IPropertyMapper<ISwipeItemMenuItem, SwipeItemMenuItemHandler> Mapper =
			new PropertyMapper<ISwipeItemMenuItem, SwipeItemMenuItemHandler>(ViewHandler.ElementMapper)
			{
				[nameof(ISwipeItemMenuItem.Visibility)] = MapVisibility,
				[nameof(IView.Background)] = MapBackground,
				[nameof(IMenuElement.Text)] = MapText,
				[nameof(ITextStyle.TextColor)] = MapTextColor,
				[nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
				[nameof(ITextStyle.Font)] = MapFont,
				[nameof(IMenuElement.Source)] = MapSource,
			};

		public static CommandMapper<ISwipeItemMenuItem, ISwipeViewHandler> CommandMapper =
			new(ElementHandler.ElementCommandMapper)
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

#if !WINDOWS
		ImageSourcePartLoader? _imageSourcePartLoader;
		public ImageSourcePartLoader SourceLoader =>
			_imageSourcePartLoader ??= new ImageSourcePartLoader(this, () => VirtualView, OnSetImageSource);

		public static void MapSource(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem image) =>
			MapSourceAsync(handler, image).FireAndForget(handler);

		public static Task MapSourceAsync(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem image)
		{
			return handler.SourceLoader.UpdateImageSourceAsync();
		}
#endif
	}
}
