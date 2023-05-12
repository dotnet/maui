#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public partial class SearchBar
	{
		[Obsolete("Use SearchBarHandler.Mapper instead.")]
		public static IPropertyMapper<ISearchBar, SearchBarHandler> ControlsSearchBarMapper =
			new PropertyMapper<SearchBar, SearchBarHandler>(SearchBarHandler.Mapper);

		static CommandMapper<ISearchBar, ISearchBarHandler> ControlsCommandMapper = new(SearchBarHandler.CommandMapper)
		{
#if ANDROID
			[nameof(ISearchBar.Focus)] = MapFocus
#endif
		};

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.SearchBar legacy behaviors
#if WINDOWS
			SearchBarHandler.Mapper.ReplaceMapping<SearchBar, ISearchBarHandler>(PlatformConfiguration.WindowsSpecific.SearchBar.IsSpellCheckEnabledProperty.PropertyName, MapIsSpellCheckEnabled);
#elif IOS
			SearchBarHandler.Mapper.ReplaceMapping<SearchBar, ISearchBarHandler>(PlatformConfiguration.iOSSpecific.SearchBar.SearchBarStyleProperty.PropertyName, MapSearchBarStyle);
#endif
			SearchBarHandler.Mapper.ReplaceMapping<SearchBar, ISearchBarHandler>(nameof(Text), MapText);
			SearchBarHandler.Mapper.ReplaceMapping<SearchBar, ISearchBarHandler>(nameof(TextTransform), MapText);

			SearchBarHandler.CommandMapper = ControlsCommandMapper;
		}
	}
}
