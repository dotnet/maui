using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 26065, "CollectionViewHandler2 null reference exception if ItemsLayout is set for Tablet but NOT Phone", PlatformAffected.iOS)]
	public partial class Issue26065 : ContentPage
	{
		public Issue26065()
		{
			InitializeComponent();
			BindingContext = new BookViewModel();
		}


		private void Button_Clicked(object sender, EventArgs e)
		{
			if (collView.ItemsLayout is LinearItemsLayout linearItemsLayout)
			{
				linearItemsLayout.ItemSpacing = 20;
			}
		}
	}

	public class Book
	{
		public string Title { get; set; }
		public string Author { get; set; }
	}

	// ViewModel
	public class BookViewModel
	{
		private ObservableCollection<Book> _bookGroups;
		public ObservableCollection<Book> Books
		{
			get => _bookGroups;
			set
			{
				_bookGroups = value;

			}
		}

		public BookViewModel()
		{
			LoadBooks();
		}

		private void LoadBooks()
		{
			Books = new ObservableCollection<Book>
			{

					new Book { Title = "Dune", Author = "Frank Herbert" },
					new Book { Title = "Neuromancer", Author = "William Gibson" },


					new Book { Title = "The Hobbit", Author = "J.R.R. Tolkien" },
					new Book { Title = "Name of the Wind", Author = "Patrick Rothfuss" },


					new Book { Title = "Murder on the Orient Express", Author = "Agatha Christie" },
					new Book { Title = "The Girl with the Dragon Tattoo", Author = "Stieg Larsson" }

			};
		}
	}

}