using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	[Preserve(AllMembers = true)]
	public class ContactsPage : ContentPage
	{
		public ContactsPage()
		{
			var listView = new ListView
			{
				ItemTemplate = new DataTemplate(() =>
				{
					var cell = new TextCell();
					cell.SetBinding(TextCell.TextProperty, new Binding("Name"));
					cell.SetBinding(TextCell.DetailProperty, new Binding("Number"));
					return cell;
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

	[Preserve(AllMembers = true)]
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

	[Preserve(AllMembers = true)]
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

	[Preserve(AllMembers = true)]
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
}