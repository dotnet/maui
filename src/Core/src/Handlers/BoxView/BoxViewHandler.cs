namespace Microsoft.Maui.Handlers
{
	public partial class BoxViewHandler
	{
		public static IPropertyMapper<IBoxView, BoxViewHandler> BoxViewMapper = new PropertyMapper<IBoxView, BoxViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IBoxView.Color)] = MapColor,
			[nameof(IBoxView.CornerRadius)] = MapCornerRadius
		};

		public BoxViewHandler() : base(BoxViewMapper)
		{

		}

		public BoxViewHandler(IPropertyMapper mapper) : base(mapper ?? BoxViewMapper)
		{

		}
	}
}