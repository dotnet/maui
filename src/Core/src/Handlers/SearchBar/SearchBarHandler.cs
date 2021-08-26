#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler
	{
		public static IPropertyMapper<ISearchBar, SearchBarHandler> SearchBarMapper = new PropertyMapper<ISearchBar, SearchBarHandler>(ViewHandler.ViewMapper)
		{
#if __ANDROID__
			[nameof(ISearchBar.Background)] = MapBackground,
#endif
			[nameof(ISearchBar.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(ISearchBar.Font)] = MapFont,
			[nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
			[nameof(ISearchBar.IsReadOnly)] = MapIsReadOnly,
			[nameof(ISearchBar.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
			[nameof(ISearchBar.MaxLength)] = MapMaxLength,
			[nameof(ISearchBar.Placeholder)] = MapPlaceholder,
			[nameof(ISearchBar.Text)] = MapText,
			[nameof(ISearchBar.TextColor)] = MapTextColor,
			[nameof(ISearchBar.CancelButtonColor)] = MapCancelButtonColor
		};

		static SearchBarHandler()
		{
#if __IOS__
			SearchBarMapper.PrependToMapping(nameof(IView.FlowDirection), (h, __) => h.UpdateValue(nameof(ITextAlignment.HorizontalTextAlignment)));
#endif
		}

		public SearchBarHandler() : base(SearchBarMapper)
		{

		}

		public SearchBarHandler(IPropertyMapper? mapper = null) : base(mapper ?? SearchBarMapper)
		{

		}
	}
}