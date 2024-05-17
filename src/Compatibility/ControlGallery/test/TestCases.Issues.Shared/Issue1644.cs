using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1644, "ListView reappearing and selecting its item causes jobject exception", PlatformAffected.Android)]
	public class Issue1644 : ContentPage
	{
		public ObservableCollection<string> Collection = new
			ObservableCollection<string>();

		public Issue1644()
		{
			for (int i = 0; i < 20; i++)
			{
				Collection.Add(DateTime.Now.ToString());
			}

			var listView = new ListView()
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				HasUnevenRows = true,
				ItemsSource = Collection,
			};

			listView.ItemSelected += (sender, e) =>
			{
				listView.SelectedItem = null;
			};

			var root = new StackLayout()
			{
				Padding = 5,
				Spacing = 5,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				Children = {
					listView
				}
			};

			Content = root;
		}
	}

	public class Issue1644Menu : FlyoutPage
	{
		Issue1644 _secondPage = new Issue1644();

		public Issue1644Menu()
		{
			var button = new Button()
			{
				Text = "MAIN MENU BUTTON"
			};

			button.Clicked += (sender, e) =>
			{
				Navigation.PushAsync(_secondPage);
			};

			Flyout = new ContentPage()
			{
				Title = "Flyout"
			};

			Detail = new ContentPage()
			{
				Title = "Detail",
				Content = button
			};
		}
	}
}

