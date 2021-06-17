using System;
using Gtk;

namespace Microsoft.Maui.Handlers
{

	public partial class SearchBarHandler : ViewHandler<ISearchBar, MauiSearchBar>
	{

		protected override MauiSearchBar CreateNativeView()
		{
			return new MauiSearchBar();
		}

		protected override void ConnectHandler(MauiSearchBar nativeView)
		{
			nativeView.Entry.Changed += OnNativeViewChanged;
		}

		protected override void DisconnectHandler(MauiSearchBar nativeView)
		{
			nativeView.Entry.Changed -= OnNativeViewChanged;
		}

		protected void OnNativeViewChanged(object? sender, EventArgs e)
		{
			if (sender != NativeView)
				return;

			NativeView?.Entry.OnTextChanged(VirtualView);
		}

		public static void MapText(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.NativeView?.Entry.UpdateText(searchBar);
		}

		public static void MapPlaceholder(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.NativeView?.Entry.UpdatePlaceholder(searchBar);

		}

		public static void MapIsReadOnly(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.NativeView?.Entry.UpdateIsReadOnly(searchBar);

		}

		public static void MapFont(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.MapFont(handler.NativeView?.Entry, searchBar);
		}

		public static void MapHorizontalTextAlignment(SearchBarHandler handler, ISearchBar searchBar)
		{
			if (handler.NativeView?.Entry is { } nativeView)
				nativeView.Alignment = searchBar.HorizontalTextAlignment.ToXyAlign();
		}

		public static void MapTextColor(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.NativeView?.Entry?.UpdateTextColor(searchBar.TextColor);

		}

		public static void MapMaxLength(SearchBarHandler handler, ISearchBar searchBar)
		{
			if (handler.NativeView?.Entry is { } nativeView)
				nativeView.MaxLength = searchBar.MaxLength;
		}

		[MissingMapper]
		public static void MapCharacterSpacing(SearchBarHandler handler, ISearchBar searchBar) { }

		[MissingMapper]
		public static void MapIsTextPredictionEnabled(SearchBarHandler handler, ISearchBar searchBar) { }
		
		[MissingMapper]
		public static void MapCancelButtonColor(IViewHandler handler, ISearchBar searchBar) { }

	}

}