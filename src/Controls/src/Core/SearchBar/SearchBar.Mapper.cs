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
#if ANDROID
			if (RuntimeFeature.IsMaterial3Enabled)
			{
				// Material3 SearchBar handler mappings
				SearchBarHandler2.Mapper.ReplaceMapping<SearchBar, SearchBarHandler2>(nameof(Text), MapText);
				SearchBarHandler2.Mapper.ReplaceMapping<SearchBar, SearchBarHandler2>(nameof(TextTransform), MapText);
			}
			else
			{
				SearchBarHandler.Mapper.ReplaceMapping<SearchBar, ISearchBarHandler>(nameof(Text), MapText);
				SearchBarHandler.Mapper.ReplaceMapping<SearchBar, ISearchBarHandler>(nameof(TextTransform), MapText);
			}
#else
			SearchBarHandler.Mapper.ReplaceMapping<SearchBar, ISearchBarHandler>(nameof(Text), MapText);
			SearchBarHandler.Mapper.ReplaceMapping<SearchBar, ISearchBarHandler>(nameof(TextTransform), MapText);
#endif

#if IOS || ANDROID
			SearchBarHandler.Mapper.AppendToMapping(nameof(VisualElement.IsFocused), InputView.MapIsFocused);
#endif

#if ANDROID
			SearchBarHandler.CommandMapper.PrependToMapping(nameof(ISearchBar.Focus), InputView.MapFocus);
#endif
		}
	}
}
