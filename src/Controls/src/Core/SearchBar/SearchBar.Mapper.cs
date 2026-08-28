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

		static readonly OneTimeInitializationAction s_remappedForControls = new(RemapForControlsOnce);
		internal override void RemapForControls()
		{
			base.RemapForControls();
			s_remappedForControls.InvokeOnce();
		}

		static void RemapForControlsOnce()
		{
			// Adjust the mappings to preserve Controls.SearchBar legacy behaviors
#if IOS
			SearchBarHandler.Mapper.ReplaceMappingForControls<SearchBar, ISearchBarHandler>(PlatformConfiguration.iOSSpecific.SearchBar.SearchBarStyleProperty.PropertyName, MapSearchBarStyle);
			SearchBarHandler.Mapper.ReplaceMappingForControls<SearchBar, ISearchBarHandler>(nameof(IsEnabled), MapUserInteraction);
			SearchBarHandler.Mapper.ReplaceMappingForControls<SearchBar, ISearchBarHandler>(nameof(ISearchBar.IsReadOnly), MapUserInteraction);
			SearchBarHandler.Mapper.ReplaceMappingForControls<SearchBar, ISearchBarHandler>(nameof(InputTransparent), MapUserInteraction);
#endif
#if ANDROID
			if (RuntimeFeature.IsMaterial3Enabled)
			{
				// Material3 SearchBar handler mappings
				SearchBarHandler2.Mapper.ReplaceMappingForControls<SearchBar, SearchBarHandler2>(nameof(Text), MapText);
				SearchBarHandler2.Mapper.ReplaceMappingForControls<SearchBar, SearchBarHandler2>(nameof(TextTransform), MapText);
			}
			else
			{
				SearchBarHandler.Mapper.ReplaceMappingForControls<SearchBar, ISearchBarHandler>(nameof(Text), MapText);
				SearchBarHandler.Mapper.ReplaceMappingForControls<SearchBar, ISearchBarHandler>(nameof(TextTransform), MapText);
			}
#else
			SearchBarHandler.Mapper.ReplaceMappingForControls<SearchBar, ISearchBarHandler>(nameof(Text), MapText);
			SearchBarHandler.Mapper.ReplaceMappingForControls<SearchBar, ISearchBarHandler>(nameof(TextTransform), MapText);
#endif

#if IOS || ANDROID
			SearchBarHandler.Mapper.AppendToMappingForControls(nameof(VisualElement.IsFocused), InputView.MapIsFocused);
#endif

#if ANDROID
			SearchBarHandler.CommandMapper.PrependToMappingForControls(nameof(ISearchBar.Focus), InputView.MapFocus);
#endif
		}
	}
}
