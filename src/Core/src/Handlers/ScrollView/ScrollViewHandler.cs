namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler
	{
		public static PropertyMapper<IScroll, ScrollViewHandler> ScrollViewMapper = new PropertyMapper<IScroll, ScrollViewHandler>(ViewMapper)
		{
			[nameof(IScroll.Content)] = MapContent
		};

		public ScrollViewHandler() : base(ScrollViewMapper)
		{

		}

		public ScrollViewHandler(PropertyMapper? mapper = null) : base(mapper ?? ScrollViewMapper)
		{

		}
	}
}