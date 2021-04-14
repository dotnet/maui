using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	internal sealed class ListViewSelectionColor : ContentPage
	{
		[Preserve(AllMembers = true)]
		internal sealed class GroupHeaderTemplate : ViewCell
		{
			public GroupHeaderTemplate()
			{
				var label = new Label { BackgroundColor = Colors.Red };
				label.SetBinding(Label.TextProperty, "Key");
				View = label;
			}
		}

		[Preserve(AllMembers = true)]
		internal sealed class GroupItemTemplate : ViewCell
		{
			public GroupItemTemplate()
			{
				var label = new Label { BackgroundColor = Colors.Green };
				label.SetBinding(Label.TextProperty, "Name");
				View = label;
			}
		}

		[Preserve(AllMembers = true)]
		internal sealed class ItemTemplate : ViewCell
		{
			public ItemTemplate()
			{
				var label = new Label { BackgroundColor = Colors.Green, HorizontalOptions = LayoutOptions.CenterAndExpand };
				label.SetBinding(Label.TextProperty, "Name");
				View = label;
			}
		}

		[Preserve(AllMembers = true)]
		internal sealed class ItemTemplate2 : ViewCell
		{
			public ItemTemplate2()
			{
				var label = new Label { BackgroundColor = Colors.Green };
				label.SetBinding(Label.TextProperty, "Name");
				var container = new StackLayout() { HorizontalOptions = LayoutOptions.FillAndExpand };
				container.Children.Add(label);
				View = container;
			}
		}

		[Preserve(AllMembers = true)]
		internal sealed class Artist
		{
			public string Name { get; private set; }

			public Artist(string name)
			{
				Name = name;
			}
		}

		[Preserve(AllMembers = true)]
		internal sealed class Grouping<K, T> : ObservableCollection<T>
		{
			public K Key { get; private set; }

			public Grouping(K key, IEnumerable<T> values)
			{
				Key = key;

				foreach (T value in values)
				{
					Items.Add(value);
				}
			}
		}

		Button _swapListButton;
		Button _changeItemTemplateButton;
		Button _clearButton;

		[Preserve(AllMembers = true)]
		public ListViewSelectionColor()
		{
			Title = "ListView ScrollTo";

			var itemSource = new[] {
				new { Name = "John Hassel" },
				new { Name = "Brian Eno" },
				new { Name = "Rober Fripp" },
				new { Name = "Edgar Froese" }
			};

			var groupedItemSource = new List<Grouping<string, Artist>> {
				new Grouping<string, Artist> ("Prog", new List<Artist> { new Artist ("King Crimson"), new Artist ("Yes"), new Artist ("Rush") }),
				new Grouping<string, Artist> ("Techno", new List<Artist> { new Artist ("Juan Atkins"), new Artist ("Jeff Mills"), new Artist ("Gerald Donald") } ),
				new Grouping<string, Artist> ("Pop", new List<Artist> { new Artist ("Japan"), new Artist ("Roxy Music"), new Artist ("Talking Heads") }),
			};

			var itemTemplate1 = new DataTemplate(typeof(ItemTemplate));
			var itemTemplate2 = new DataTemplate(typeof(ItemTemplate2));
			var normalList = new ListView
			{
				ItemTemplate = itemTemplate1,
				ItemsSource = itemSource
			};
			var groupedList = new ListView
			{
				IsGroupingEnabled = true,
				GroupHeaderTemplate = new DataTemplate(typeof(GroupHeaderTemplate)),
				ItemTemplate = new DataTemplate(typeof(GroupItemTemplate)),
				ItemsSource = groupedItemSource
			};

			var currentList = normalList;
			_swapListButton = new Button
			{
				Text = "Swap List Type",
				Command = new Command(() =>
				{
					if (currentList == normalList)
					{
						SetContent(groupedList);
						currentList = groupedList;
					}
					else
					{
						SetContent(normalList);
						currentList = normalList;
					}
				})
			};

			var currentTemplate = normalList.ItemTemplate;
			_changeItemTemplateButton = new Button
			{
				Text = "Change Item template",
				Command = new Command(() =>
				{
					if (currentTemplate == itemTemplate1)
					{
						normalList.ItemTemplate = itemTemplate2;
						currentTemplate = itemTemplate2;
					}
					else
					{
						normalList.ItemTemplate = itemTemplate1;
						currentTemplate = itemTemplate1;
					}
				})
			};

			_clearButton = new Button
			{
				Text = "Clear",
				Command = new Command(() =>
				{
					currentList.SelectedItem = null;
				})
			};

			Content = new StackLayout
			{
				Children = {
					new StackLayout {
						Orientation = StackOrientation.Horizontal,
						Children = {
							_swapListButton,
							_changeItemTemplateButton,
							_clearButton
						}
					},
					normalList
				}
			};

		}

		void SetContent(ListView list)
		{
			Content = new StackLayout
			{
				Children = {
					_swapListButton,
					_changeItemTemplateButton,
					_clearButton,
					list
				}
			};
		}


	}
}