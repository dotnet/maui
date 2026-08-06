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

using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeItemMenuItemHandler : ISwipeItemMenuItemHandler
	{
		static readonly ConditionalWeakTable<ISwipeItemMenuItemHandler, HandlerState> s_handlerStates = new();

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

		// The tint is resolved while the icon is being applied to the platform view, so reloading the
		// source is what re-evaluates it. This also guarantees a previously applied tint is cleared
		// rather than left stale when IconColor changes (for example on an app theme switch).
		// Route through UpdateValue so user mapper customizations for Source (AppendToMapping /
		// PrependToMapping) are preserved instead of bypassed by a direct MapSource call.
		public static void MapIconColor(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			if (handler.IsMappingProperties())
				return;

			handler.UpdateValue(nameof(IMenuElement.Source));
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
				handler.UpdateValue(nameof(IMenuElement.Source));
			}
		}

		internal static HandlerState GetHandlerState(ISwipeItemMenuItemHandler handler) =>
			s_handlerStates.GetValue(handler, static _ => new HandlerState());

		internal static int BeginIconLoad(ISwipeItemMenuItemHandler handler) =>
			System.Threading.Interlocked.Increment(ref GetHandlerState(handler).IconLoadGeneration);

		internal static bool IsIconLoadCurrent(
			ISwipeItemMenuItemHandler handler,
			ISwipeItemMenuItem item,
			object platformView,
			int generation) =>
			generation == System.Threading.Volatile.Read(ref GetHandlerState(handler).IconLoadGeneration) &&
			ReferenceEquals(handler.VirtualView, item) &&
			ReferenceEquals(handler.PlatformView, platformView);

		public static Task MapSourceAsync(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem image)
		{
#if WINDOWS
			return LoadFileIconAsync(handler, image);
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

		internal sealed class HandlerState
		{
			public int IconLoadGeneration;
		}
	}
}
