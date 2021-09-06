#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class PageHandler : IViewHandler
	{
		public static IPropertyMapper<IView, PageHandler> PageMapper = new PropertyMapper<IView, PageHandler>(ViewMapper)
		{
			[nameof(ITitledElement.Title)] = MapTitle,
			[nameof(IContentView.Content)] = MapContent,
		};

		public static CommandMapper<IPicker, PickerHandler> PageCommandMapper = new(ViewCommandMapper)
		{
#if __IOS__
			[nameof(IView.Frame)] = MapFrame,
#endif
		};

		public PageHandler() : base(PageMapper, PageCommandMapper)
		{

		}

		public PageHandler(IPropertyMapper? mapper = null) : base(mapper ?? PageMapper)
		{

		}
	}
}
