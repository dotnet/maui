#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class PageHandler : IViewHandler
	{
		public static PropertyMapper<IPage, PageHandler> PageMapper = new PropertyMapper<IPage, PageHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IPage.Title)] = MapTitle,
			[nameof(IPage.Content)] = MapContent,
		};

#if __IOS__
		public static CommandMapper<IPicker, PickerHandler> PageCommandMapper = new(ViewCommandMapper)
		{
			[nameof(IFrameworkElement.Frame)] = MapFrame,
		};

		public PageHandler() : base(PageMapper, PageCommandMapper)
#else
		public PageHandler() : base(PageMapper)
#endif

		{

		}

		public PageHandler(PropertyMapper? mapper = null) : base(mapper ?? PageMapper)
		{

		}
	}
}
