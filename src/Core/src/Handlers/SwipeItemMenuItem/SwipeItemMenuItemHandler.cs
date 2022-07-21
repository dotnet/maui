#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIButton;
#elif MONOANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.SwipeItem;
#elif TIZEN
using PlatformView = ElmSharp.EvasObject;
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
		ISwipeItemMenuItem ISwipeItemMenuItemHandler.VirtualView => VirtualView;

		PlatformView ISwipeItemMenuItemHandler.PlatformView => PlatformView;

#if !WINDOWS
		ImageSourcePartLoader? _imageSourcePartLoader;
		public ImageSourcePartLoader SourceLoader =>
			_imageSourcePartLoader ??= new ImageSourcePartLoader(this, () => VirtualView, OnSetImageSource);


		public static void MapSource(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem image) =>
			MapSourceAsync(handler, image).FireAndForget(handler);

		public static Task MapSourceAsync(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem image)
		{
			if (handler is SwipeItemMenuItemHandler platformHandler)
				return platformHandler.SourceLoader.UpdateImageSourceAsync();
			return Task.CompletedTask;
		}

#endif
	}
}
