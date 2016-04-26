using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	[Preserve (AllMembers = true)]
	public class GroupedListContactsGallery
		: ContentPage
	{
		internal class Contact
		{
			public string FirstName
			{
				get;
				set;
			}

			public string LastName
			{
				get;
				set;
			}

			public string FullName
			{
				get { return FirstName + " " + LastName; }
			}

			public string Title
			{
				get;
				set;
			}
		}

		readonly ListView _list = new ListView {
			ItemTemplate = new DataTemplate (() => {
				Label name = new Label();
				name.SetBinding (Label.TextProperty, "FullName");

#pragma warning disable 618
				Label title = new Label { Font = Font.SystemFontOfSize (NamedSize.Micro) };
#pragma warning restore 618
				title.SetBinding (Label.TextProperty, "Title");

				return new ViewCell { View = new StackLayout  {
					Children = {
						name,
						title
					}
				} };
			}),

			GroupDisplayBinding = new Binding ("Key"),
			GroupShortNameBinding = new Binding ("Key"),
			IsGroupingEnabled = true
		};

		readonly List<Contact> _contacts = new List<Contact> {
			new Contact { FirstName = "Jason", LastName = "Smith", Title = "Software Engineer" },
			new Contact { FirstName = "Eric", LastName = "Maupin", Title = "Software Engineer" },
			new Contact { FirstName = "Seth", LastName = "Rosetter", Title = "Software Engineer"  },
			new Contact { FirstName = "Stephane", LastName = "Delcroix", Title = "Software Engineer" }
		};

		readonly Random _rand = new Random (42);

		[Preserve (AllMembers = true)]
		internal class Group
			: ObservableCollection<Contact>
		{
			public Group (string key)
			{
				Key = key;
			}

			public string Key
			{
				get;
				private set;
			}
		}

		bool _sortedByFirst = true;
		ObservableCollection<Group> _sortedContacts;

		public GroupedListContactsGallery()
		{
			var addRandom = new Button { Text = "Random" };
			addRandom.Clicked += (sender, args) => {
				Contact contact = GetRandomContact();
				_contacts.Add (contact);

				AddContact (_sortedContacts, contact);
			};

			var addRandomToExisting = new Button { Text = "Random Group" };
			addRandomToExisting.Clicked += (sender, args) => {
				Contact contact;
				do {
					contact = GetRandomContact();
				} while (!_sortedContacts.Any (g => GetSortChar (g.First()) == GetSortChar (contact)));

				_contacts.Add (contact);
				AddContact (_sortedContacts, contact);
			};

			var groupByFirst = new Button { Text = "First" };
			groupByFirst.Clicked += (sender, args) => {
				_sortedByFirst = true;

				SetupContacts();
				_list.ItemsSource = _sortedContacts;
			};

			var groupByLast = new Button { Text = "Last" };
			groupByLast.Clicked += (sender, args) => {
				_sortedByFirst = false;

				SetupContacts();
				_list.ItemsSource = _sortedContacts;
			};

			Content = new StackLayout {
				Orientation = StackOrientation.Vertical,
				Children = {
					new StackLayout {
						Orientation = StackOrientation.Vertical,
						Children = {
							new StackLayout {
								Orientation = StackOrientation.Horizontal,
								Children = {
									new Label { Text = "Sort: " },
									groupByFirst,
									groupByLast
								}
							},

							new StackLayout {
								Orientation = StackOrientation.Horizontal,
								Children = {
									addRandom,
									addRandomToExisting,
								}
							}
						},
					},
					

					_list
				}
			};

			SetupContacts();
			_list.ItemsSource = _sortedContacts;
		}

		const string Chars = "abcdefghijklmnopqrstuvwxyz";

		void SetupContacts()
		{
			var coll = new ObservableCollection<Group>();
			foreach (var contact in _contacts)
				AddContact (coll, contact);

			_sortedContacts = coll;
		}

		void AddContact (ObservableCollection<Group> contactGroups, Contact contact)
		{
			char sortChar = GetSortChar (contact);

			var collection = contactGroups.FirstOrDefault (col => {
				var c = col.First();
				return (GetSortChar (c) == sortChar);
			});

			if (collection == null) {
				var ocontacts = new Group (GetSortChar (contact).ToString()) { contact };
				InsertBasedOnSort (contactGroups, ocontacts, c => GetSortChar (c.First()));
			} else
				InsertBasedOnSort (collection, contact, c => GetSortString (c)[0]);
		}

		int IndexOf<T> (IEnumerable<T> elements, T element)
		{
			int i = 0;
			foreach (T e in elements) {
				if (Equals (e, element))
					return i;

				i++;
			}

			return -1;
		}

		void InsertBasedOnSort<T,TSort> (IList<T> items, T item, Func<T, TSort> sortBy)
		{
			List<T> newItems = new List<T> (items);
			newItems.Add (item);
			int index = IndexOf (newItems.OrderBy (sortBy), item);
			items.Insert (index, item);
		}

		char GetSortChar (Contact contact)
		{
			return GetSortString (contact)[0];
		}

		string GetSortString (Contact contact)
		{
			return (_sortedByFirst) ? contact.FirstName : contact.LastName;
		}

		Contact GetRandomContact()
		{
			Contact contact = new Contact();

			int firstLen = _rand.Next (3, 7);
			
			var builder = new StringBuilder (firstLen);
			for (int i = 0; i < firstLen; i++) {
				char c = Chars[_rand.Next (0, Chars.Length)];
				builder.Append ((i != 0) ? c : char.ToUpper (c));
			}
			
			contact.FirstName = builder.ToString();

			int lastLen = _rand.Next (3, 7);
			builder.Clear();
			for (int i = 0; i < lastLen; i++) {
				char c = Chars[_rand.Next (0, Chars.Length)];
				builder.Append ((i != 0) ? c : char.ToUpper (c));
			}

			contact.LastName = builder.ToString();
			contact.Title = "Software Engineer";

			return contact;
		}
	}
}
