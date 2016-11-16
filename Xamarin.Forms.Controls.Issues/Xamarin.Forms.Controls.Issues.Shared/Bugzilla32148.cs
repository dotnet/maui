using System;
using Xamarin.Forms.CustomAttributes;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;

using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ListView)]
#endif

	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 32148, " Pull to refresh hides the first item on a list view")]
	public class Bugzilla32148 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		Button _searchBtn;
		ListView _contactsListView;
		ObservableCollection<Grouping1<string, ContactViewModel1>> _listViewItemSource;

		protected override void Init ()
		{
			Title = "Contacts";
			Content = CreateContent();
			LoadContactsAsync();
		}

		Layout CreateContent()
		{
			_listViewItemSource = new ObservableCollection<Grouping1<string, ContactViewModel1>>();

			_contactsListView = new ListView()
			{
				ItemsSource = _listViewItemSource,
				IsPullToRefreshEnabled = true,
				IsGroupingEnabled = true,
				GroupShortNameBinding = new Binding("Key"),
				GroupHeaderTemplate = new DataTemplate(typeof(HeaderCell)),
				HasUnevenRows = true,
				ItemTemplate = new DataTemplate(typeof(ContactItemTemplate))
			};

			_contactsListView.Refreshing += contactsListView_Refreshing;
			_searchBtn = new Button() { Text = "Search" };
			_searchBtn.Clicked += (object sender, EventArgs e) => _contactsListView.BeginRefresh ();

			Grid grd = new Grid ();
			grd.RowDefinitions.Add (new RowDefinition () { Height = GridLength.Auto } );
			grd.RowDefinitions.Add (new RowDefinition ());
			grd.Children.Add (_searchBtn);
			grd.Children.Add (_contactsListView);
			Grid.SetRow (_contactsListView, 1);
			return grd;
		}

		async void contactsListView_Refreshing(object sender, EventArgs e)
		{
			await Task.Delay (1000);
			await LoadContactsAsync(true);
		}

		async Task LoadContactsAsync(bool isPullToRefresh = false)
		{
			await ReadFromDbAsync();
			_contactsListView.IsRefreshing &= !isPullToRefresh;
		}

		async Task ReadFromDbAsync(Expression<Func<Contact1, bool>> searchExpression = null)
		{
				await Task.Run(() =>
				{
					Device.BeginInvokeOnMainThread(() =>
						{
							// If we want to filter the data, GetItems by expression
							IList<Contact1> contactEntities = new List<Contact1>();
							List<ContactViewModel1> data = new List<ContactViewModel1>();

							if (contactEntities == null || contactEntities.Count == 0)
							{
								// Fill with dummy contacts
								for (int i = 0; i < 20; i++)
								{
									Contact1 contact = new Contact1()
									{
										FirstName = "Contact" + i,
										LastName = "LastName",
										Company = "Company" + i
									} ;
									contactEntities.Add(contact);
								}
							}

							// Create Contact items for the listView
							foreach (Contact1 contact in contactEntities)
							{
								ContactViewModel1 contactItem = new ContactViewModel1()
								{
									FirstName = contact.FirstName,
									LastName = contact.LastName,
									FullName = contact.FirstName + " " + contact.LastName,
									Company = contact.Company,
								} ;

								data.Add(contactItem);
							}

							// Sort, order and group the contacts
							var contacts = from contact in data
								orderby contact.LastName, contact.FirstName
							group contact by contact.FirstNameSort into contactGroup
							select new Grouping1<string, ContactViewModel1>(contactGroup.Key, contactGroup);

							// Create a new collection of groups
							var grouppedContacts = new ObservableCollection<Grouping1<string, ContactViewModel1>>(contacts);

							_contactsListView.ItemsSource = grouppedContacts;
						} );
				} );
		}

		public class Grouping1<K, T> : ObservableCollection<T>
		{
			public K Key { get; private set; }

			public Grouping1(K key, IEnumerable<T> items)
			{
				Key = key;
				foreach (T item in items)
				{
					Items.Add(item);
				}
			}
		}

		[Preserve (AllMembers = true)]
		public class ContactViewModel1
		{
			public string FirstName { get; set; }
			public string LastName { get; set; }
			public string FullName { get; set; }
			public string Company { get; set; }
			public ImageSource IconSource { get; set; }

			public string FirstNameSort
			{
				get
				{
					if (string.IsNullOrWhiteSpace(FirstName) || FirstName.Length == 0)
					{
						return "?";
					}

					return FirstName[0].ToString().ToUpper();
				}
			}
		}

		[Preserve (AllMembers = true)]
		public class Contact1
		{
			public string FirstName { get; set; }
			public string LastName { get; set; }
			public string Company { get; set; }
			public byte[] ProfilePicture { get; set; }
			public string Email { get; set; }
			public string Mobile { get; set; }
			public string RoomNumber { get; set; }
			public string Street { get; set; }
			public string Zip { get; set; }
			public string City { get; set; }
			public string CountryCode { get; set; }
		}

		[Preserve (AllMembers = true)]
		public class HeaderCell : ViewCell
		{
			public HeaderCell()
			{
				Height = 23;

				Label title = new Label
				{
					TextColor = Color.White,
					VerticalOptions = LayoutOptions.Center
				} ;

				title.SetBinding(Label.TextProperty, "Key");

				View = new StackLayout
				{
					HorizontalOptions = LayoutOptions.FillAndExpand,
					HeightRequest = 23,
					BackgroundColor = Color.Pink,
					Orientation = StackOrientation.Horizontal,
					Padding = new Thickness(Sizes.GroupingSidePadding, 0, 0, 0),
					Children = { title }
				};
			}

			struct Sizes
			{
				public static readonly double GroupingSidePadding = 5;
			}
		}

		[Preserve (AllMembers = true)]
		public class ContactItemTemplate : ImageCell
		{
			public ContactItemTemplate()
				: base()
			{

				SetBinding(TextProperty, new Binding("FullName"));
				SetBinding(DetailProperty, new Binding("Company"));
				SetBinding(ImageSourceProperty, new Binding("IconSource"));

				Height = 50;
			}
		}

		#if UITEST
		[Test]
		public void Bugzilla32148Test ()
		{
			RunningApp.WaitForElement (q => q.Marked ("Contact0 LastName"));
			RunningApp.Tap (q => q.Marked("Search"));
			RunningApp.WaitForElement (q => q.Marked ("Contact0 LastName"));
			RunningApp.Screenshot ("For manual review, is the first cell visible?");
		}
		#endif
	}

}
