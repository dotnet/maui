using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 31964, "[Xamarin.Forms SwitchCell]OnChange() did not work for Windows platform",
		PlatformAffected.WinRT)]
	public class Bugzilla31964 : TestContentPage
	{
		protected override void Init ()
		{
			var listView = new ListView ();

			var selection = new Selection ();
			listView.SetBinding (ListView.ItemsSourceProperty, "Items");

			listView.ItemTemplate = new DataTemplate (() => {
				var cell = new SwitchCell ();
				cell.SetBinding (SwitchCell.TextProperty, "Name");
				cell.SetBinding (SwitchCell.OnProperty, "IsSelected", BindingMode.TwoWay);
				return cell;
			});

			var instructions = new Label {
				FontSize = 16,
				Text =
					"The label at the bottom should equal the number of switches which are in the 'on' position. Flip some of the switches. If the number at the bottom does not equal the number of 'on' switches, the test has failed."
			};

			var label = new Label { FontSize = 24 };
			label.SetBinding (Label.TextProperty, "SelectedCount");

			Content = new StackLayout {
				VerticalOptions = LayoutOptions.Fill,
				Children = {
					instructions,
					listView,
					label
				}
			};

			BindingContext = selection;
		}

		[Preserve (AllMembers = true)]
		public class SelectionItem : INotifyPropertyChanged
		{
			bool _isSelected;

			public string Name { get; set; }

			public bool IsSelected
			{
				get { return _isSelected; }
				set
				{
					_isSelected = value;
					OnPropertyChanged ();
				}
			}

			public event PropertyChangedEventHandler PropertyChanged;

			void OnPropertyChanged ([CallerMemberName] string propertyName = null)
			{
				PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
			}
		}

		[Preserve (AllMembers = true)]
		public class Selection : INotifyPropertyChanged
		{
			int _selectedCount;

			public Selection ()
			{
				Items = new List<SelectionItem> {
					new SelectionItem { Name = "Item1" },
					new SelectionItem { Name = "Item2" },
					new SelectionItem { Name = "Item3", IsSelected = true },
					new SelectionItem { Name = "Item4" },
					new SelectionItem { Name = "Item5" }
				};

				SelectedCount = Items.Count (i => i.IsSelected);

				foreach (SelectionItem selectionItem in Items) {
					selectionItem.PropertyChanged += (sender, args) => SelectedCount = Items.Count (i => i.IsSelected);
				}
			}

			public List<SelectionItem> Items { get; }

			public int SelectedCount
			{
				get { return _selectedCount; }
				set
				{
					_selectedCount = value;
					OnPropertyChanged ();
				}
			}

			public event PropertyChangedEventHandler PropertyChanged;

			void OnPropertyChanged ([CallerMemberName] string propertyName = null)
			{
				PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
			}
		}
	}
}
