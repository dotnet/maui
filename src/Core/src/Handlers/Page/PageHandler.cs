#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class PageHandler : ContentViewHandler
	{
		public static IPropertyMapper<IContentView, PageHandler> PageMapper = new PropertyMapper<IPage, PageHandler>(ContentViewMapper)
		{
			[nameof(ITitledElement.Title)] = MapTitle,
			[nameof(IPage.BackgroundImageSource)] = MapBackgroundImageSource
		};

		public static CommandMapper<IPage, PageHandler> PageCommandMapper = new(ContentViewCommandMapper)
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
