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

		public static Factory PageViewFactory { get; set; } = new Factory();

		public static FactoryMapper<IView, PageHandler> FactoryMapper = new()
		{
			[nameof(Factory.CreateNativeView)] = (v, h, _) => PageViewFactory.CreateNativeView(v, h),
#if __IOS__
			[nameof(Factory.CreateViewController)] = (v, h, _) => PageViewFactory.CreateViewController(v, h)
#endif
		};

		public new partial class Factory : ViewHandler.Factory
		{
		}


		public PageHandler() : base(PageMapper, PageCommandMapper)
		{

		}

		public PageHandler(IPropertyMapper? mapper = null) : base(mapper ?? PageMapper)
		{

		}
	}
}
