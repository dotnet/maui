using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 29512, "[Windows] CollectionView Navigation Crash and Display Bug in .NET 9", PlatformAffected.UWP)]
	public class Issue29512 : NavigationPage
	{
		public Issue29512() : base(new Issue29512Page1())
		{
		}
	}

	public class Issue29512Group
	{
		public required string Name { get; set; }
		public List<Issue29512Item> Items { get; set; } = [];
	}

	public class Issue29512Item
	{
		public required string Title { get; set; }
		public required Issue29512Group Group { get; set; }
	}

	public class Issue29512Page1 : ContentPage
	{
		public IList<Issue29512Group> AllGroups { get; set; }
		public IList<Issue29512Item> AllItems { get; set; }

		public Issue29512Group SelectedGroup { get; set; }
		public Issue29512Item SelectedItem { get; set; }

		public Issue29512Page1()
		{
			// Mock data
			var groupA = new Issue29512Group { Name = "Group A" };
			var groupB = new Issue29512Group { Name = "Group B" };
			var item1 = new Issue29512Item { Title = "Item 1", Group = groupA };
			var item2 = new Issue29512Item { Title = "Item 2", Group = groupA };
			var item3 = new Issue29512Item { Title = "Item 3", Group = groupB };
			groupA.Items.AddRange([item1, item2]);
			groupB.Items.Add(item3);

			AllGroups = [groupA, groupB];
			AllItems = [item1, item2, item3];

			var browseButton = new Button { Text = "Browse", AutomationId = "BrowseButton" };
			browseButton.Clicked += (s, e) =>
			{
				if (AllItems.Count > 1)
					NavigateForward(new Issue29512Page2(this));
				else
					NavigateForward(new Issue29512Page3(this, true, false));
			};

			Content = new StackLayout
			{
				Children = { browseButton }
			};
		}

		public async void NavigateForward(ContentPage page)
		{
			await Navigation.PushModalAsync(page);
		}

		public async void CloseBrowsing()
		{
			await Navigation.PopModalAsync();
		}

		public async void GoBack()
		{
			await Navigation.PopModalAsync();
		}
	}

	public class Issue29512Page2 : ContentPage
	{
		readonly Issue29512Page1 _itemsFilter;
		readonly CollectionView _listView;

		public Issue29512Page2(Issue29512Page1 libraryFilter)
		{
			_itemsFilter = libraryFilter;

			_listView = new CollectionView
			{
				ItemsSource = _itemsFilter.AllGroups,
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, nameof(Issue29512Group.Name));
					return new StackLayout
					{
						Children = { label },
						Padding = new Thickness(10),
						Margin = new Thickness(10),
					};
				}),
				SelectionMode = SelectionMode.Single,
			};

			_listView.SelectionChanged += (s, e) =>
			{
				if (e.CurrentSelection.FirstOrDefault() is Issue29512Group selectedGroup)
				{
					_itemsFilter.SelectedGroup = selectedGroup;

					if (selectedGroup.Items.Count > 1)
						_itemsFilter.NavigateForward(new Issue29512Page3(_itemsFilter, false, true));
					else
						_itemsFilter.CloseBrowsing();
				}
				_listView.SelectedItem = null;
			};

			var closeButton = new Button { Text = "Close", VerticalOptions = LayoutOptions.End };
			closeButton.Clicked += (s, e) => _itemsFilter.CloseBrowsing();

			Content = new StackLayout
			{
				Children = {
					new Label {
						Text = "Browse Libraries",
						FontSize = 24,
						FontAttributes = FontAttributes.Bold,
						TextColor = Colors.Aqua,
						HorizontalOptions = LayoutOptions.Center,
						Margin = new Thickness(0, 20, 0, 10)
					},
					_listView,
					closeButton
				}
			};
		}
	}

	public class Issue29512Page3 : ContentPage
	{
		readonly Issue29512Page1 _libraryFilter;
		readonly CollectionView _listView;

		public Issue29512Page3(Issue29512Page1 libraryFilter, bool isAllLibraries, bool shouldShowBack)
		{
			_libraryFilter = libraryFilter;

			IList<Issue29512Item> books = isAllLibraries ? _libraryFilter.AllItems : _libraryFilter.SelectedGroup?.Items ?? new List<Issue29512Item>();

			_listView = new CollectionView
			{
				ItemsSource = books,
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, nameof(Issue29512Item.Title));
					return new StackLayout
					{
						Children = { label },
						Padding = new Thickness(10),
						Margin = new Thickness(10),
					};
				}),
				SelectionMode = SelectionMode.Single
			};

			_listView.SelectionChanged += (s, e) =>
			{
				if (e.CurrentSelection.FirstOrDefault() is Issue29512Item selectedItem)
				{
					_libraryFilter.SelectedItem = selectedItem;
					_libraryFilter.CloseBrowsing();
				}
				_listView.SelectedItem = null;
			};

			var closeButton = new Button { Text = "Close" };
			closeButton.Clicked += (s, e) => _libraryFilter.CloseBrowsing();

			var stack = new StackLayout
			{
				Children = {
					new Label {
						Text = "Browse Items",
						FontSize = 24,
						FontAttributes = FontAttributes.Bold,
						TextColor = Colors.DarkGray,
						HorizontalOptions = LayoutOptions.Center,
						Margin = new Thickness(0, 20, 0, 10)
					}
				}
			};

			if (shouldShowBack)
			{
				var backButton = new Button { Text = "Back" };
				backButton.Clicked += (s, e) => _libraryFilter.GoBack();
				stack.Children.Add(backButton);
			}

			stack.Children.Add(_listView);
			stack.Children.Add(closeButton);

			Content = stack;
		}
	}
}