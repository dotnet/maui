#nullable enable
#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiSearchBar;
#elif MONOANDROID
using PlatformView = AndroidX.AppCompat.Widget.SearchView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.AutoSuggestBox;
#elif TIZEN
using PlatformView = Microsoft.Maui.Platform.MauiSearchBar;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler : ISearchBarHandler
	{
		public static IPropertyMapper<ISearchBar, ISearchBarHandler> Mapper = new PropertyMapper<ISearchBar, ISearchBarHandler>(ViewHandler.ViewMapper)
		{
#if __IOS__
			[nameof(ISearchBar.IsEnabled)] = MapIsEnabled,
#endif
			[nameof(ISearchBar.Background)] = MapBackground,
			[nameof(ISearchBar.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(ISearchBar.Font)] = MapFont,
			[nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
			[nameof(ITextAlignment.VerticalTextAlignment)] = MapVerticalTextAlignment,
			[nameof(ISearchBar.IsReadOnly)] = MapIsReadOnly,
			[nameof(ISearchBar.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
			[nameof(ISearchBar.IsSpellCheckEnabled)] = MapIsSpellCheckEnabled,
			[nameof(ISearchBar.MaxLength)] = MapMaxLength,
			[nameof(ISearchBar.Placeholder)] = MapPlaceholder,
			[nameof(ISearchBar.PlaceholderColor)] = MapPlaceholderColor,
			[nameof(ISearchBar.Text)] = MapText,
			[nameof(ISearchBar.TextColor)] = MapTextColor,
			[nameof(ISearchBar.CancelButtonColor)] = MapCancelButtonColor,
			[nameof(ISearchBar.Keyboard)] = MapKeyboard,
			[nameof(ISearchBar.ReturnType)] = MapReturnType
		};

		public static CommandMapper<ISearchBar, ISearchBarHandler> CommandMapper = new(ViewCommandMapper)
		{
#if ANDROID
			[nameof(ISearchBar.Focus)] = MapFocus
#endif
		};

		public SearchBarHandler() : this(Mapper)
		{
		}

		public SearchBarHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public SearchBarHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		ISearchBar ISearchBarHandler.VirtualView => VirtualView;

		PlatformView ISearchBarHandler.PlatformView => PlatformView;
	}
}