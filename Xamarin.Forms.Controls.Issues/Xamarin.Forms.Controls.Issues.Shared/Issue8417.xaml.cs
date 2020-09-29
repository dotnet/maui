using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.UITest.iOS;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8417, "(Android) CarouselView Java.Lang.IllegalStateException", PlatformAffected.Android)]
	public partial class Issue8417 : TestContentPage
	{
		public Issue8417()
		{
#if APP
			InitializeComponent();
			BindingContext = new Issue8417ViewModel();
#endif
		}

		protected override void Init()
		{

		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			await Task.Delay(2000);
#if APP
			carouselView.ItemsSource = (BindingContext as Issue8417ViewModel).Items;
#endif
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue8417Model
	{
		public string Name { get; set; }
		public string Location { get; set; }
		public string Details { get; set; }
		public string ImageUrl { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class Issue8417ViewModel : BindableObject
	{
		readonly IList<Issue8417Model> _items;

		public ObservableCollection<Issue8417Model> Items { get; private set; }

		public Issue8417ViewModel()
		{
			_items = new List<Issue8417Model>();

			CreateCollection();
		}

		void CreateCollection()
		{
			_items.Add(new Issue8417Model
			{
				Name = "Baboon",
				Location = "Africa & Asia",
				Details = "Baboons are African and Arabian Old World monkeys belonging to the genus Papio, part of the subfamily Cercopithecinae.",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/f/fc/Papio_anubis_%28Serengeti%2C_2009%29.jpg/200px-Papio_anubis_%28Serengeti%2C_2009%29.jpg"
			});

			_items.Add(new Issue8417Model
			{
				Name = "Capuchin Monkey",
				Location = "Central & South America",
				Details = "The capuchin monkeys are New World monkeys of the subfamily Cebinae. Prior to 2011, the subfamily contained only a single genus, Cebus.",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/4/40/Capuchin_Costa_Rica.jpg/200px-Capuchin_Costa_Rica.jpg"
			});

			_items.Add(new Issue8417Model
			{
				Name = "Blue Monkey",
				Location = "Central and East Africa",
				Details = "The blue monkey or diademed monkey is a species of Old World monkey native to Central and East Africa, ranging from the upper Congo River basin east to the East African Rift and south to northern Angola and Zambia",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/8/83/BlueMonkey.jpg/220px-BlueMonkey.jpg"
			});

			Items = new ObservableCollection<Issue8417Model>(_items);
		}
	}
}