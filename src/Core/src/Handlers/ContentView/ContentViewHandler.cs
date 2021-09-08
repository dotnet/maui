#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class ContentViewHandler
	{
		public static IPropertyMapper<IContentView, ContentViewHandler> ContentViewMapper = new PropertyMapper<IContentView, ContentViewHandler>(ViewMapper)
		{
			[nameof(IContentView.Content)] = MapContent,
		};

		public static CommandMapper<IPicker, PickerHandler> ContentViewCommandMapper = new(ViewCommandMapper)
		{
#if __IOS__
			[nameof(IView.Frame)] = MapFrame,
#endif
		};

		public ContentViewHandler() : base(ContentViewMapper, ContentViewCommandMapper)
		{

		}

		protected ContentViewHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper ?? ViewCommandMapper)
		{
		}

		public ContentViewHandler(IPropertyMapper? mapper = null) : base(mapper ?? ContentViewMapper)
		{

		}
	}
}
