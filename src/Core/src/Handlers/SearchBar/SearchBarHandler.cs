namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler
	{
		public static PropertyMapper<ISearchBar, SearchBarHandler> SearchBarMapper = new PropertyMapper<ISearchBar, SearchBarHandler>(ViewHandler.ViewMapper)
		{
			[nameof(ISearchBar.Text)] = MapText,
			[nameof(ISearchBar.Placeholder)] = MapPlaceholder,
			[nameof(ISearchBar.Font)] = MapFont,
			[nameof(ISearchBar.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
			[nameof(ISearchBar.CharacterSpacing)] = MapCharacterSpacing
		};

		public SearchBarHandler() : base(SearchBarMapper)
		{

		}

		public SearchBarHandler(PropertyMapper? mapper = null) : base(mapper ?? SearchBarMapper)
		{

		}
	}
}