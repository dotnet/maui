using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 32148, " Pull to refresh hides the first item on a list view")]
public class Bugzilla32148 : TestContentPage
{
	Button _searchBtn;
	ListView _contactsListView;
	ObservableCollection<Grouping1<string, ContactViewModel1>> _listViewItemSource;

	protected override void Init()
	{
		Title = "Contacts";
		Content = CreateContent();

#pragma warning disable 4014
		LoadContactsAsync();
#pragma warning restore 4014
	}

	Layout CreateContent()
	{
		_listViewItemSource = new ObservableCollection<Grouping1<string, ContactViewModel1>>();

		_contactsListView = new ListView()
		{
			ItemsSource = _listViewItemSource,
			IsPullToRefreshEnabled = true,
			IsGroupingEnabled = true,
			// While using this GroupShortNameBinding property it throws an exception on Windows
			// for more information:https://github.com/dotnet/maui/issues/26534. For this test case we don't need this property.
			// GroupShortNameBinding = new Binding("Key"),
			GroupHeaderTemplate = new DataTemplate(typeof(HeaderCell)),
			HasUnevenRows = true,
			ItemTemplate = new DataTemplate(typeof(ContactItemTemplate))
		};

		_contactsListView.Refreshing += contactsListView_Refreshing;
		_searchBtn = new Button() { Text = "Search" };
		_searchBtn.Clicked += (object sender, EventArgs e) => _contactsListView.BeginRefresh();

		Grid grd = new Grid();
		grd.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
		grd.RowDefinitions.Add(new RowDefinition());
		grd.Children.Add(_searchBtn);
		grd.Children.Add(_contactsListView);
		Grid.SetRow(_contactsListView, 1);
		return grd;
	}

	async void contactsListView_Refreshing(object sender, EventArgs e)
	{
		await Task.Delay(1000);
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
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
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
							};
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
						};

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
				});
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
		});
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

				return FirstName[0].ToString().ToUpperInvariant();
			}
		}
	}


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


	public class HeaderCell : ViewCell
	{
		public HeaderCell()
		{
			Height = 23;

			Label title = new Label
			{
				TextColor = Colors.White,
				VerticalOptions = LayoutOptions.Center
			};

			title.SetBinding(Label.TextProperty, "Key");

#pragma warning disable CS0618 // Type or member is obsolete
			View = new StackLayout
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				HeightRequest = 23,
				BackgroundColor = Colors.Pink,
				Orientation = StackOrientation.Horizontal,
				Padding = new Thickness(Sizes.GroupingSidePadding, 0, 0, 0),
				Children = { title }
			};
#pragma warning restore CS0618 // Type or member is obsolete
		}

		struct Sizes
		{
			public static readonly double GroupingSidePadding = 5;
		}
	}


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
}
