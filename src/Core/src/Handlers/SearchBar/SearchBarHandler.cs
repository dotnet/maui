#nullable enable
#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiSearchBar;
#elif MONOANDROID
using PlatformView = AndroidX.AppCompat.Widget.SearchView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.AutoSuggestBox;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.ElmSharp.SearchBar;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler : ISearchBarHandler
	{
		public static IPropertyMapper<ISearchBar, ISearchBarHandler> Mapper = new PropertyMapper<ISearchBar, ISearchBarHandler>(ViewHandler.ViewMapper)
		{
#if __ANDROID__ || __WINDOWS__
			[nameof(ISearchBar.Background)] = MapBackground,
#elif __IOS__
			[nameof(ISearchBar.IsEnabled)] = MapIsEnabled,
#endif
			[nameof(ISearchBar.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(ISearchBar.Font)] = MapFont,
			[nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
			[nameof(ITextAlignment.VerticalTextAlignment)] = MapVerticalTextAlignment,
			[nameof(ISearchBar.IsReadOnly)] = MapIsReadOnly,
			[nameof(ISearchBar.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
			[nameof(ISearchBar.MaxLength)] = MapMaxLength,
			[nameof(ISearchBar.Placeholder)] = MapPlaceholder,
			[nameof(ISearchBar.PlaceholderColor)] = MapPlaceholderColor,
			[nameof(ISearchBar.Text)] = MapText,
			[nameof(ISearchBar.TextColor)] = MapTextColor,
			[nameof(ISearchBar.CancelButtonColor)] = MapCancelButtonColor
		};

		public static CommandMapper<ISearchBar, ISearchBarHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		static SearchBarHandler()
		{
#if __IOS__
			Mapper.PrependToMapping(nameof(IView.FlowDirection), (h, __) => h.UpdateValue(nameof(ITextAlignment.HorizontalTextAlignment)));
#endif
		}

		public SearchBarHandler() : base(Mapper)
		{
		}

		public SearchBarHandler(IPropertyMapper? mapper = null) : base(mapper ?? Mapper)
		{
		}

		ISearchBar ISearchBarHandler.VirtualView => VirtualView;

		PlatformView ISearchBarHandler.PlatformView => PlatformView;
	}
}