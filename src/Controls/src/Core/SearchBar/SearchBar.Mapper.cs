#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class SearchBar
	{
		static SearchBar()
		{
			// Register dependency: SearchCommand depends on SearchCommandParameter for CanExecute evaluation
			// See https://github.com/dotnet/maui/issues/31939
			SearchCommandProperty.DependsOn(SearchCommandParameterProperty);
		}

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.SearchBar legacy behaviors
#if IOS
			SearchBarHandler.Mapper.ReplaceMapping<SearchBar, ISearchBarHandler>(PlatformConfiguration.iOSSpecific.SearchBar.SearchBarStyleProperty.PropertyName, MapSearchBarStyle);
#endif
			SearchBarHandler.Mapper.ReplaceMapping<SearchBar, ISearchBarHandler>(nameof(Text), MapText);
			SearchBarHandler.Mapper.ReplaceMapping<SearchBar, ISearchBarHandler>(nameof(TextTransform), MapText);

#if IOS || ANDROID
			SearchBarHandler.Mapper.AppendToMapping(nameof(VisualElement.IsFocused), InputView.MapIsFocused);
#endif

#if ANDROID
			SearchBarHandler.CommandMapper.PrependToMapping(nameof(ISearchBar.Focus), InputView.MapFocus);
#endif
		}
	}
}
