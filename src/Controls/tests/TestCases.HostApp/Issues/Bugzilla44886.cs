using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 44886, "UWP Listview ItemSelected event triggered twice for each selection", PlatformAffected.UWP)]
public class Bugzilla44886 : TestContentPage
{
	const string Item1 = "Item 1";
	const string Instructions = "Select one of the items in the list. The text in blue should show 1, indicating that the ItemSelected event fired once. If it shows 2, this test has failed. Be sure to also test Keyboard selection and Narrator selection. On UWP, the ItemSelected event should fire when an item is highlighted and again when it is un-highlighted (by pressing spacebar).";
	const string CountId = "countId";

	Label _CountLabel = new Label { AutomationId = CountId, TextColor = Colors.Blue };
	MyViewModel _vm = new MyViewModel();


	class MyViewModel : INotifyPropertyChanged
	{
		int _count;
		public int Count
		{
			get { return _count; }
			set
			{
				if (value != _count)
				{
					_count = value;
					RaisePropertyChanged();
				}
			}
		}

		void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;

			handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#region INotifyPropertyChanged implementation

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}

	protected override void Init()
	{
		BindingContext = _vm;

		_CountLabel.SetBinding(Label.TextProperty, nameof(MyViewModel.Count));

		var listView = new ListView
		{
			ItemsSource = new List<string> { Item1, "Item 2", "Item 3", "Item 4", "Item 5" }
		};
		listView.ItemTemplate = new DataTemplate(() =>
		{
			var cell = new TextCell();
			cell.SetBinding(TextCell.TextProperty, ".");
			cell.SetBinding(TextCell.AutomationIdProperty, ".");
			return cell;
		});
		listView.ItemSelected += ListView_ItemSelected;

		var stack = new StackLayout { Children = { new Label { Text = Instructions }, _CountLabel, listView } };
		Content = stack;
	}

	void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
	{
		if (e.SelectedItem == null)
		{
			return; //ItemSelected is called on deselection, which results in SelectedItem being set to null
		}

		_vm.Count++;

		ListView lst = (ListView)sender;
		lst.SelectedItem = null;
	}
}