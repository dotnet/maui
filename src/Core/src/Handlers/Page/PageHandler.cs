#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class PageHandler : ContentViewHandler
	{
		public static IPropertyMapper<IContentView, PageHandler> PageMapper = new PropertyMapper<IContentView, PageHandler>(Mapper)
		{
			[nameof(ITitledElement.Title)] = MapTitle
		};

		public static CommandMapper<IContentView, PageHandler> PageCommandMapper = new(CommandMapper)
		{

		};

		public PageHandler() : base(PageMapper, PageCommandMapper)
		{

		}

		public PageHandler(IPropertyMapper? mapper = null) : base(mapper ?? PageMapper)
		{

		}
	}
}
