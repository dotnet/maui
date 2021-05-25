#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler
	{
		public static PropertyMapper<ISearchBar, SearchBarHandler> SearchBarMapper = new PropertyMapper<ISearchBar, SearchBarHandler>(ViewHandler.ViewMapper)
		{
#if __ANDROID__
			[nameof(ISearchBar.Background)] = MapBackground,
#endif
			[nameof(ISearchBar.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(ISearchBar.Font)] = MapFont,
			[nameof(ISearchBar.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
			[nameof(ISearchBar.IsReadOnly)] = MapIsReadOnly,
			[nameof(ISearchBar.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
			[nameof(ISearchBar.MaxLength)] = MapMaxLength,
			[nameof(ISearchBar.Placeholder)] = MapPlaceholder,
			[nameof(ISearchBar.Text)] = MapText,
			[nameof(ISearchBar.TextColor)] = MapTextColor,
		};

		public SearchBarHandler() : base(SearchBarMapper)
		{

		}

		public SearchBarHandler(PropertyMapper? mapper = null) : base(mapper ?? SearchBarMapper)
		{

		}
	}
}