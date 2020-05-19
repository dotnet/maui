using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class SearchBarGallery : ContentPage
	{
		Label _searchQueryLabel;

		public SearchBarGallery ()
		{
			_searchQueryLabel = new Label {
				Text = "Search Query"
			};

			var textChangedLabel = new Label {
				Text = ""
			};

			var numCharsTyped = 0;

			var noPlaceholder = new SearchBar();
			noPlaceholder.SearchButtonPressed += OnSearchBarPressed;
			noPlaceholder.TextChanged += (sender, e) => {
				numCharsTyped++;
				textChangedLabel.Text = numCharsTyped.ToString ();
			};

			var normal = new SearchBar { Placeholder = "Normal" };
			normal.SearchButtonPressed += OnSearchBarPressed;
			normal.TextChanged += (sender, e) => {
				numCharsTyped++;
				textChangedLabel.Text = numCharsTyped.ToString ();	
			};

			var activation = new SearchBar { Placeholder = "Activation" };
			activation.SearchButtonPressed += OnSearchBarPressed;
			activation.TextChanged += (sender, e) => {
				numCharsTyped++;
				textChangedLabel.Text = numCharsTyped.ToString ();
			};

			var nextPageButton = new Button {
				Text = "More SearchBars",
				Command = new Command (async () => await Navigation.PushAsync (new SearchBarGalleryPageTwo ()))
			};

			int i = 1;
			activation.SearchButtonPressed += (sender, e) => {
				activation.Text = "";
				activation.Placeholder = "Activated " + i++.ToString ();
			};

			Content = new ScrollView { 
				Content = new StackLayout {
					Spacing = 0,
					Children = {
						noPlaceholder,
						normal,
						activation,
						_searchQueryLabel,
						textChangedLabel,
						nextPageButton
					}
				}
			};
		}

		void OnSearchBarPressed(object sender, EventArgs eventArgs)
		{
			var searchBar = (SearchBar)sender;
			_searchQueryLabel.Text = searchBar.Text;
		}
	}

	public class SearchBarGalleryPageTwo : ContentPage
	{
		Label _searchQueryLabel;

		public SearchBarGalleryPageTwo ()
		{
			Title = "Search Bar Gallery Part 2";

			_searchQueryLabel = new Label {
				Text = "Search Query 2"
			};

			var disabled = new SearchBar { Placeholder = "Disabled", IsEnabled = false };
			disabled.SearchButtonPressed += OnSearchBarPressed;
			var transparent = new SearchBar { Placeholder = "Transparent", Opacity = 0.5 };
			transparent.SearchButtonPressed += OnSearchBarPressed;

			var toggleDisabledButton = new Button {
				Text = "Toggle enabled",
				Command = new Command (() => {
					if (disabled.IsEnabled) {
						disabled.IsEnabled = false;
						disabled.Placeholder = "Disabled";
					} else {
						disabled.IsEnabled = true;
						disabled.Placeholder = "Enabled";
					}
				})
			};

			Content = new ScrollView { 
				Content = new StackLayout {
					Spacing = 0,
					Children = {
						disabled,
						transparent,
						_searchQueryLabel,
						toggleDisabledButton
					}
				}
			};
		}

		void OnSearchBarPressed(object sender, EventArgs eventArgs)
		{
			var searchBar = (SearchBar)sender;
			_searchQueryLabel.Text = searchBar.Text;
		}
	}
}
