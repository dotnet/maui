using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	internal class GroupedListActionsGallery
		: ContentPage
	{
		class Group
			: ObservableList<GroupAction>, INotifyPropertyChanged
		{
			public Group (string name)
			{
				if (name == null)
					throw new ArgumentNullException ("name");

				Name = name;
				ShortName = Name[0].ToString();
			}

			string _name;
			string _shortName;
			public event PropertyChangedEventHandler PropertyChanged;

			public string Name
			{
				get { return _name; }
				set
				{
					if (_name == value)
						return;

					_name = value;
					OnPropertyChanged();
				}
			}

			public string ShortName
			{
				get { return _shortName; }
				set
				{
					if (_shortName == value)
						return;

					_shortName = value;
					OnPropertyChanged();
				}
			}

			protected override void InsertItem (int index, GroupAction item)
			{
				item.Parent = this;
				base.InsertItem (index, item);
			}

			void OnPropertyChanged ([CallerMemberName] string propertyName = null)
			{
				PropertyChangedEventHandler handler = PropertyChanged;
				if (handler != null)
					handler (this, new PropertyChangedEventArgs (propertyName));
			}
		}

		class GroupAction
		{
			readonly Action<GroupAction> _action;

			public GroupAction (string name, Action<GroupAction> action = null)
			{
				if (name == null)
					throw new ArgumentNullException ("name");
				
				Name = name;
				_action = action;
			}

			public string Name
			{
				get;
				private set;
			}

			public void DoStuff()
			{
				if (_action == null)
					return;

				_action (this);
			}

			public Group Parent
			{
				get;
				set;
			}
		}

		readonly ListView _list = new ListView {
			IsGroupingEnabled = true,
			GroupHeaderTemplate = new DataTemplate(() => {
				var label = new Label {
					VerticalOptions = LayoutOptions.CenterAndExpand,
					TextColor = Color.Red
				};

				label.SetBinding (Label.TextProperty, "Name");

				return new ViewCell {
					View = new StackLayout {
						Orientation = StackOrientation.Horizontal,
						Children = {
							new Image {
								Source = ImageSource.FromFile ("cover1.jpg"),
								HeightRequest = 20,
								WidthRequest = 20
							},

							label,
						}
					}
				};
			}),

			ItemTemplate = new DataTemplate (() => {
				var c = new TextCell();
				c.SetBinding (TextCell.TextProperty, "Name");
				return c;
			})
		};

		readonly ObservableList<Group> _groups;

		ObservableList<Group> CreateItemSource()
		{
			return new ObservableList<Group> {
				new Group ("General") {
					new GroupAction ("Change group name", ga => ga.Parent.Name += " (changed)"),
					new GroupAction ("Change group short name", ga => ga.Parent.ShortName = ga.Parent.Name[1].ToString())
				},

				new Group ("Child item actions") {
					new GroupAction ("Clear this group", ga => ga.Parent.Clear()),
					new GroupAction ("Insert group item", ga => ga.Parent.Insert (1, new GroupAction ("Inserted item S"))),
					new GroupAction ("Insert 2 group items", ga => ga.Parent.InsertRange (1, GetGroupActions ("Inserted item D", 2))),
					new GroupAction ("Remove next item", ga => ga.Parent.Remove (ga.Parent[GetIndexOfDummy (ga)])),
					new GroupAction ("Dummy item RDI"),
					new GroupAction ("Remove next 2 dummy items", ga => ga.Parent.RemoveRange (GetNextDummyItems (ga, 2))),
					new GroupAction ("Dummy item RmDI-1"),
					new GroupAction ("Dummy item RmDI-2"),
					new GroupAction ("Replace dummy item", ga => ga.Parent[GetIndexOfDummy(ga)] = new GroupAction ("Replaced item")),
					new GroupAction ("Dummy item RpDI"),
					new GroupAction ("Replace next two dummy items", ga =>
						ga.Parent.ReplaceRange (GetIndexOfDummy (ga, 2), GetGroupActions ("Replaced items", 2))),
					new GroupAction ("Dummy item RpDI-1"),
					new GroupAction ("Dummy item RpDI-2"),
					new GroupAction ("Select next dummy item", ga => {
						int index = GetIndexOfDummy (ga);
						_list.SelectedItem = ga.Parent[index];
					}),
					new GroupAction ("Dummy item SI"),
					new GroupAction ("Move dummy above this one", ga => {
						int nindex = GetIndexOfDummy (ga);
						ga.Parent.Move (nindex, ga.Parent.IndexOf (c => c.Name.StartsWith ("Move dummy")));
					}),
					new GroupAction ("Dummy item MDI"),
					new GroupAction ("Move last 2 items above this one", ga => {
						int nindex = GetIndexOfDummy (ga, 2);
						ga.Parent.Move (nindex, ga.Parent.IndexOf (c => c.Name.StartsWith ("Move last 2")), 2);
					}),
					new GroupAction ("Dummy item M2DI-1"),
					new GroupAction ("Dummy item M2DI-2"),
				},

				CreateDummyGroup (2),
				CreateDummyGroup (2),

				new Group ("Group item actions") {
					new GroupAction ("Clear all", ga => _groups.Clear()),
					new GroupAction ("Insert group", ga => {
						int index = _groups.IndexOf (g => g.Name == "Group item actions");
						_groups.Insert (index, CreateDummyGroup (2));
					}),

					new GroupAction ("Insert 2 groups", ga => {
						int index = _groups.IndexOf (g => g.Name == "Group item actions");
						_groups.InsertRange (index, new[] { CreateDummyGroup (2), CreateDummyGroup (2) });
					}),

					new GroupAction ("Remove previous dummy group", ga => {
						int index = _groups.IndexOf (g => g.Name == "Group item actions");
						var dg = _groups.Take (index).Last (g => g.Name == "Dummy group");
						_groups.Remove (dg);
					}),

					new GroupAction ("Remove previous 2 dummy groups", ga => {
						int index = _groups.IndexOf (g => g.Name == "Group item actions");
						var dgs = _groups.Take (index).Reverse().Where (g => g.Name == "Dummy group").Take (2);
						_groups.RemoveRange (dgs);
					}),

					new GroupAction ("Replace previous dummy group", ga => {
						int index = _groups.IndexOf (g => g.Name == "Group item actions");
						var dg = _groups.Take (index).Last (g => g.Name == "Dummy group");
						_groups[_groups.IndexOf (dg)] = new Group ("Replaced group") {
							new GroupAction ("Replaced group item")
						};
					}),

					new GroupAction ("Replace previous 2 dummy groups", ga => {
						int index = _groups.IndexOf (g => g.Name == "Group item actions");
						var dgs = _groups.Take (index).Reverse().Where (g => g.Name == "Dummy group").Take (2).Reverse();
						_groups.ReplaceRange (_groups.IndexOf (dgs.First()), new[] {
							new Group ("Replaced group") {
								new GroupAction ("Replaced group item")
							},
							new Group ("Replaced group") {
								new GroupAction ("Replaced group item")
							}
						});
					}),

					new GroupAction ("Move next group above", ga => {
						int index = _groups.IndexOf (g => g.Name == ga.Parent.Name);
						int dgindex = _groups.IndexOf (_groups.Skip (index + 1).First (dg => dg.Name == "Dummy group"));
						_groups.Move (dgindex, index);
					}),

					new GroupAction ("Move next 2 groups above", ga => {
						int index = _groups.IndexOf (g => g.Name == ga.Parent.Name);
						int dgindex = _groups.IndexOf (_groups.Skip (index + 1).First (dg => dg.Name == "Dummy group"));
						_groups.Move (dgindex, index, 2);
					}),
				},

				CreateDummyGroup (1),
				CreateDummyGroup (1),
				CreateDummyGroup (1)
			};
		}

		public GroupedListActionsGallery()
		{
			NavigationPage.SetHasNavigationBar (this, false);
			_groups = CreateItemSource();

			_list.ItemTapped += (sender, arg) => ((GroupAction)arg.Item).DoStuff();

			_list.ItemsSource = _groups;
			Title = "Actions";
			Content = _list;
		}

		IEnumerable<GroupAction> GetGroupActions (string name, int count)
		{
			return Enumerable.Range (0, count).Select (i => new GroupAction (name + " " + i));
		}

		Group CreateDummyGroup (int children)
		{
			var group = new Group ("Dummy group");
			group.AddRange (Enumerable.Range (0, children).Select (i => new GroupAction ("Dummy item")));
			return group;
		}

		int GetIndexOfDummy (GroupAction source, int count = 1)
		{
			var dummies = GetNextDummyItems (source, count);
			return source.Parent.IndexOf (dummies.First());
		}

		IEnumerable<GroupAction> GetNextDummyItems (GroupAction source, int count)
		{
			int start = source.Parent.IndexOf (source);
			return source.Parent.Skip (start).Where (xga => xga.Name.StartsWith ("Dummy item")).Take (count);
		}
	}
}
