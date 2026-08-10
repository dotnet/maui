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

		public static void MapSource(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem image) =>
			MapSourceAsync(handler, image).FireAndForget(handler);

		public static Task MapSourceAsync(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem image)
		{
#if WINDOWS
			// TODO: make the mapper use the loader and the image if this is a stream source
			handler.PlatformView.IconSource = image.Source?.ToIconSource(handler.MauiContext!);
#else
			if (handler.SourceLoader is ImageSourcePartLoader loader)
				return loader.UpdateImageSourceAsync();
#endif
			return Task.CompletedTask;
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
