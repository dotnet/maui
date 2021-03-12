using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.CollectionViewGalleries.GroupingGalleries
{
	internal class ObservableGrouping : ContentPage
	{
		public ObservableGrouping()
		{
			Title = "Observable Grouped List";

			var buttonStyle = new Style(typeof(Button)) { };
			buttonStyle.Setters.Add(new Setter() { Property = Button.HeightRequestProperty, Value = 30 });
			buttonStyle.Setters.Add(new Setter() { Property = Button.FontSizeProperty, Value = 10 });

			var layout = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				},
				ColumnDefinitions = new ColumnDefinitionCollection
				{
					new ColumnDefinition(), new ColumnDefinition()
				}

			};

			var collectionView = new CollectionView
			{
				Header = "This is a header",
				Footer = "Hey, I'm a footer. Look at me!",
				ItemTemplate = ItemTemplate(),
				GroupFooterTemplate = GroupFooterTemplate(),
				GroupHeaderTemplate = GroupHeaderTemplate(),
				IsGrouped = true,
				SelectionMode = SelectionMode.Single
			};

			var itemsSource = new ObservableSuperTeams();

			collectionView.ItemsSource = itemsSource;

			var remover = new Button { Text = "Remove Selected", AutomationId = "RemoveItem", Style = buttonStyle };
			remover.Clicked += (obj, args) =>
			{
				var selectedMember = collectionView.SelectedItem as Member;
				var team = FindTeam(itemsSource, selectedMember);
				team?.Remove(selectedMember);
			};

			var adder = new Button { Text = "Add After Selected", AutomationId = "AddItem", Style = buttonStyle };
			adder.Clicked += (obj, args) =>
			{
				var selectedMember = collectionView.SelectedItem as Member;
				var team = FindTeam(itemsSource, selectedMember);

				if (team == null)
				{
					return;
				}

				team.Insert(team.IndexOf(selectedMember) + 1, new Member("Spider-Man"));
			};

			AddStuffToGridRow(layout, 0, remover, adder);

			var replacer = new Button { Text = "Replace Selected", AutomationId = "ReplaceItem", Style = buttonStyle };
			replacer.Clicked += (obj, args) =>
			{
				var selectedMember = collectionView.SelectedItem as Member;
				var team = FindTeam(itemsSource, selectedMember);

				if (team == null)
				{
					return;
				}

				team.Insert(team.IndexOf(selectedMember) + 1, new Member("Spider-Man"));
				team.Remove(selectedMember);
			};

			var mover = new Button
			{
				Text = $"Move Selected To {itemsSource[0].Name}",
				AutomationId = "MoveItem",
				Style = buttonStyle
			};
			mover.Clicked += (obj, args) =>
			{
				var selectedMember = collectionView.SelectedItem as Member;
				var team = FindTeam(itemsSource, selectedMember);

				if (team == null || team == itemsSource[0])
				{
					return;
				}

				team.Remove(selectedMember);
				itemsSource[0].Add(selectedMember);
			};

			AddStuffToGridRow(layout, 1, replacer, mover);

			var groupRemover = new Button
			{
				Text = $"Remove {itemsSource[0].Name}",
				AutomationId = "RemoveGroup",
				Style = buttonStyle
			};
			groupRemover.Clicked += (obj, args) =>
			{
				itemsSource?.Remove(itemsSource[0]);
				if (itemsSource.Count > 0)
				{
					groupRemover.Text = $"Remove {itemsSource[0].Name}";
				}
				else
				{
					groupRemover.Text = "";
					groupRemover.IsEnabled = false;
				}
				mover.Text = $"Move Selected To {itemsSource[0].Name}";
			};

			var groupAdder = new Button
			{
				Text = $"Insert New Group at position 2",
				AutomationId = "AddGroup",
				Style = buttonStyle
			};
			groupAdder.Clicked += (obj, args) =>
			{
				itemsSource?.Insert(1, new ObservableTeam("Excalibur", new List<Member>()));
			};

			AddStuffToGridRow(layout, 2, groupRemover, groupAdder);

			var groupMover = new Button { Text = "Move 3rd Group to 1st", AutomationId = "MoveGroup", Style = buttonStyle };
			groupMover.Clicked += (obj, args) =>
			{
				var group = itemsSource[2];
				itemsSource.Remove(group);
				itemsSource.Insert(0, group);
				groupRemover.Text = $"Remove {itemsSource[0].Name}";
				mover.Text = $"Move Selected To {itemsSource[0].Name}";
			};

			var groupReplacer = new Button { Text = "Replace 2nd Group", AutomationId = "ReplaceGroup", Style = buttonStyle };
			groupReplacer.Clicked += (obj, args) =>
			{
				var group = itemsSource[1];
				itemsSource.Remove(group);
				itemsSource?.Insert(1, new ObservableTeam("Alpha Flight", new List<Member> { new Member("Guardian"),
					new Member("Sasquatch"), new Member("Northstar") }));
			};

			AddStuffToGridRow(layout, 3, groupMover, groupReplacer);

			layout.Children.Add(collectionView);
			Grid.SetRow(collectionView, 6);
			Grid.SetColumnSpan(collectionView, 2);

			Content = layout;
		}

		void AddStuffToGridRow(Grid grid, int row, params View[] views)
		{
			var col = 0;

			foreach (var view in views)
			{
				grid.Children.Add(view);
				Grid.SetRow(view, row);
				Grid.SetColumn(view, col);
				col = col + 1;
			}
		}

		DataTemplate ItemTemplate()
		{
			return new DataTemplate(() =>
			{
				var layout = new StackLayout();
				var label = new Label()
				{
					Margin = new Thickness(5, 0, 0, 0),

				};

				label.SetBinding(Label.TextProperty, new Binding("Name"));
				layout.Children.Add(label);

				return layout;
			});
		}

		DataTemplate GroupHeaderTemplate()
		{
			return new DataTemplate(() =>
			{
				var layout = new StackLayout();
				var label = new Label()
				{
					FontSize = 16,
					FontAttributes = FontAttributes.Bold,
					BackgroundColor = Color.LightGreen

				};

				label.SetBinding(Label.TextProperty, new Binding("Name"));
				layout.Children.Add(label);

				return layout;
			});
		}

		DataTemplate GroupFooterTemplate()
		{
			return new DataTemplate(() =>
			{
				var layout = new StackLayout();
				var label = new Label()
				{
					Margin = new Thickness(0, 0, 0, 15),
					BackgroundColor = Color.LightBlue
				};

				label.SetBinding(Label.TextProperty, new Binding("Count", stringFormat: "Total members: {0:D}"));
				layout.Children.Add(label);

				return layout;
			});
		}

		ObservableTeam FindTeam(ObservableSuperTeams teams, Member member)
		{
			if (member == null)
			{
				return null;
			}

			for (int i = 0; i < teams.Count; i++)
			{
				var group = teams[i];

				if (group.Contains(member))
				{
					return group;
				}
			}

			return null;
		}
	}
}
