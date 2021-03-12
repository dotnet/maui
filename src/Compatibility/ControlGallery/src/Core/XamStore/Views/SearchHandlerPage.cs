using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using static Microsoft.Maui.Controls.Compatibility.ControlGallery.DynamicViewGallery;
using static Microsoft.Maui.Controls.Compatibility.ControlGallery.XamStore.BasePage;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.XamStore
{
	[Preserve(AllMembers = true)]
	public class SearchHandlerPage : ContentPage
	{
		SearchHandler _searchHandler;
		StackLayout _propertyLayout;
		const string SearchHandlerKey = nameof(SearchHandler);

		public SearchHandlerPage()
		{
			On<iOS>().SetUseSafeArea(true);

			TestedTypes.Add(SearchHandlerKey, (AddSearchHandler, new NamedAction[] { new NamedAction { Name = nameof(Focus), Action = FocusUnfocusSearchHandler } }));

			_searchHandler = TestedTypes[SearchHandlerKey].ctor() as SearchHandler;

			_propertyLayout = new StackLayout
			{
				Spacing = 10,
				Padding = 10,
				Children = { new Button { Text = "Show/Hide SearchHandler", Command = new Command(() => ShowHideSearchHandler()) } }
			};
			Content = new StackLayout
			{
				Children = {
					new Label { Text = "Test SearchHandler on IOs that this text appears", HorizontalTextAlignment = TextAlignment.Center },
					new ScrollView { Content = _propertyLayout }
				}
			};

			GetProperties(_searchHandler, typeof(SearchHandler), _propertyLayout);

		}

		void FocusUnfocusSearchHandler(object searchHandler)
		{
			var sh = searchHandler as SearchHandler;
			if (sh == null)
				return;

			if (sh.IsFocused)
				sh.Unfocus();
			else
				sh.Focus();
		}

		void ShowHideSearchHandler()
		{
			if (_searchHandler == null)
			{
				_searchHandler = TestedTypes[SearchHandlerKey].ctor() as SearchHandler;
			}
			else
			{
				_searchHandler = null;

				Shell.SetSearchHandler(this, _searchHandler);
			}
		}

		internal CustomSearchHandler AddSearchHandler()
		{
			var searchHandler = new CustomSearchHandler();

			searchHandler.BackgroundColor = Color.Orange;
			searchHandler.CancelButtonColor = Color.Pink;
			searchHandler.TextColor = Color.White;
			searchHandler.PlaceholderColor = Color.Yellow;
			searchHandler.HorizontalTextAlignment = TextAlignment.Center;
			searchHandler.ShowsResults = true;

			searchHandler.Keyboard = Keyboard.Numeric;

			searchHandler.FontFamily = "ChalkboardSE-Regular";
			searchHandler.FontAttributes = FontAttributes.Bold;

			searchHandler.ClearIconName = "Clear";
			searchHandler.ClearIconHelpText = "Clears the search field text";

			searchHandler.ClearPlaceholderName = "Voice Search";
			searchHandler.ClearPlaceholderHelpText = "Start voice search";

			searchHandler.QueryIconName = "Search";
			searchHandler.QueryIconHelpText = "Press to search app";

			searchHandler.Placeholder = "Type to search";
			searchHandler.ClearPlaceholderEnabled = true;
			searchHandler.ClearPlaceholderIcon = "mic.png";

			Shell.SetSearchHandler(this, searchHandler);
			return searchHandler;
		}

	}
}

