#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class PageHandler : IViewHandler
	{
		public static PropertyMapper<IView, PageHandler> PageMapper = new(ViewMapper)
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

		public PageHandler(PropertyMapper? mapper = null) : base(mapper ?? PageMapper)
		{

		}
	}
}
