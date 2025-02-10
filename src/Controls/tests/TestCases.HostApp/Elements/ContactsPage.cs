using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

public class ContactsPage : ContentPage
{
	public ContactsPage()
	{
		var listView = new ListView
		{
			ItemTemplate = new DataTemplate(() =>
			{
				var stackLayout = new VerticalStackLayout() { Spacing = 4 };

				var nameLabel = new Label() { FontSize = 14 };
				nameLabel.SetBinding(Label.TextProperty, "Name");
				nameLabel.SetBinding(Label.AutomationIdProperty, "Name");

				var numberLabel = new Label() { FontSize = 10 };
				numberLabel.SetBinding(Label.TextProperty, "Number");
				numberLabel.SetBinding(Label.AutomationIdProperty, "Number");

				stackLayout.Children.Add(nameLabel);
				stackLayout.Children.Add(numberLabel);

				return new ViewCell() { View = stackLayout };
			}),
			IsGroupingEnabled = true,
			GroupDisplayBinding = new Binding("Name")
		};

		var groupedContacts = new ObservableCollection<Group<ContactViewModel>> {
			new Group<ContactViewModel> ("E", new[] {
				new ContactViewModel { Name = "Egor1", Number = "'Tap' on this item won't fire the event" },
				new ContactViewModel { Name = "Egor2", Number = "123" },
				new ContactViewModel { Name = "Egor3", Number = "123" },
			})
		};

		listView.ItemsSource = groupedContacts;
		listView.ItemTapped += ListViewOnItemTapped;

		Content = listView;
	}

	void ListViewOnItemTapped(object sender, ItemTappedEventArgs itemTappedEventArgs)
	{
		DisplayActionSheet("Tapped a List item", "Cancel", "Destruction");
	}
}


public class ContactViewModel : ViewModelBase2
{
	string _name;
	string _number;

	public string Name
	{
		get { return _name; }
		set { SetProperty(ref _name, value); }
	}

	public string Number
	{
		get { return _number; }
		set { SetProperty(ref _number, value); }
	}
}

public class Group<TItem> : ObservableCollection<TItem>
{
	public Group(string name, IEnumerable<TItem> items)
	{
		Name = name;
		foreach (var item in items)
			Add(item);
	}

	public string Name { get; set; }
}

public class ViewModelBase2 : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual ViewModelBase2 SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
	{
		field = value;
		PropertyChangedEventHandler handler = PropertyChanged;
		if (handler != null)
			handler(this, new PropertyChangedEventArgs(propertyName));
		return this;
	}
}