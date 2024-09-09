#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class PageHandler : ContentViewHandler, IPageHandler
	{
		public static new IPropertyMapper<IContentView, IPageHandler> Mapper =
			new PropertyMapper<IContentView, IPageHandler>(ContentViewHandler.Mapper)
			{
#if IOS || TIZEN
				[nameof(IContentView.Background)] = MapBackground,
#if IOS
				[nameof(IiOSPageSpecifics.IsHomeIndicatorAutoHidden)] = MapHomeIndicatorAutoHidden,
				[nameof(IiOSPageSpecifics.PrefersStatusBarHiddenMode)] = MapPrefersStatusBarHiddenMode,
#endif
#endif
				[nameof(ITitledElement.Title)] = MapTitle,
			};

		public static new CommandMapper<IContentView, IPageHandler> CommandMapper =
			new(ContentViewHandler.CommandMapper);

		public PageHandler() : base(Mapper, CommandMapper)
		{
		}

		public PageHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public PageHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}
	}
}
