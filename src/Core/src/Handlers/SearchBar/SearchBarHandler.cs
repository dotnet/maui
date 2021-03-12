namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler
	{
		public static PropertyMapper<ISearchBar, SearchBarHandler> SearchBarMapper = new PropertyMapper<ISearchBar, SearchBarHandler>(ViewHandler.ViewMapper)
		{
			[nameof(ISearchBar.Text)] = MapText
		};

		public SearchBarHandler() : base(SearchBarMapper)
		{

		}

		public SearchBarHandler(PropertyMapper mapper) : base(mapper ?? SearchBarMapper)
		{

		}
	}
}