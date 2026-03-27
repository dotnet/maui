using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 34666, "The C6 page cannot scroll on Windows and Android platforms", PlatformAffected.All)]
	public class Issue34666 : ContentPage
	{
		public Issue34666()
		{
			Title = "C6";

			var collectionView = new CollectionView
			{
				AutomationId = "CollectionView",
				VerticalScrollBarVisibility = ScrollBarVisibility.Always,
				ItemTemplate = new DataTemplate(() =>
				{
					var itemGrid = new Grid { Padding = new Thickness(10) };
					itemGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
					itemGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
					itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
					itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

					var image = new Image
					{
						Aspect = Aspect.AspectFill,
						HeightRequest = 60,
						WidthRequest = 60
					};
					image.SetBinding(Image.SourceProperty, nameof(Issue34666Support.Monkey.ImageUrl));
					Grid.SetRowSpan(image, 2);

					var nameLabel = new Label { FontAttributes = FontAttributes.Bold };
					nameLabel.SetBinding(Label.TextProperty, nameof(Issue34666Support.Monkey.Name));
					nameLabel.SetBinding(Label.AutomationIdProperty, nameof(Issue34666Support.Monkey.Name));
					Grid.SetColumn(nameLabel, 1);

					var locationLabel = new Label
					{
						FontAttributes = FontAttributes.Italic,
						VerticalOptions = LayoutOptions.End
					};
					locationLabel.SetBinding(Label.TextProperty, nameof(Issue34666Support.Monkey.Location));
					Grid.SetRow(locationLabel, 1);
					Grid.SetColumn(locationLabel, 1);

					itemGrid.Add(image);
					itemGrid.Add(nameLabel);
					itemGrid.Add(locationLabel);

					return itemGrid;
				})
			};

			collectionView.SetBinding(CollectionView.ItemsSourceProperty, nameof(Issue34666Support.MonkeysViewModel.Monkeys));

			var refreshView = new RefreshView
			{
				IsEnabled = false,
				Content = collectionView
			};

			var mainGrid = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				}
			};
			mainGrid.Add(new Label { Text = "1. Pull down to initiate a refresh." }, 0, 0);
			mainGrid.Add(new Label { Text = "2. The test passes if the progress spinner does not appear." }, 0, 1);
			mainGrid.Add(refreshView, 0, 2);

			Content = mainGrid;
			BindingContext = new Issue34666Support.MonkeysViewModel();
		}
	}
}

namespace Maui.Controls.Sample.Issues.Issue34666Support
{
	// Exact code from Sandbox: src/Controls/samples/Controls.Sample.Sandbox/Models/Monkey.cs
	public class Monkey
	{
		public string Name { get; set; } = string.Empty;

		public string Location { get; set; } = string.Empty;

		public string Details { get; set; } = string.Empty;

		public string ImageUrl { get; set; } = string.Empty;

		public bool IsFavorite { get; set; }
	}

	// Exact code from Sandbox: src/Controls/samples/Controls.Sample.Sandbox/ViewModels/MonkeysViewModel.cs
	public class MonkeysViewModel : INotifyPropertyChanged
	{
		readonly IList<Monkey> source;

		public ObservableCollection<Monkey> Monkeys { get; set; } = [];

		public MonkeysViewModel()
		{
			source = new List<Monkey>();
			CreateMonkeyCollection();
		}

		void CreateMonkeyCollection()
		{
			source.Add(new Monkey
			{
				Name = "Baboon",
				Location = "Africa & Asia",
				Details = "Baboons are African and Arabian Old World monkeys belonging to the genus Papio, part of the subfamily Cercopithecinae.",
				ImageUrl = "papio.jpg"
			});

			source.Add(new Monkey
			{
				Name = "Capuchin Monkey",
				Location = "Central & South America",
				Details = "The capuchin monkeys are New World monkeys of the subfamily Cebinae. Prior to 2011, the subfamily contained only a single genus, Cebus.",
				ImageUrl = "capuchin.jpg"
			});

			source.Add(new Monkey
			{
				Name = "Blue Monkey",
				Location = "Central and East Africa",
				Details = "The blue monkey or diademed monkey is a species of Old World monkey native to Central and East Africa, ranging from the upper Congo River basin east to the East African Rift and south to northern Angola and Zambia",
				ImageUrl = "bluemonkey.jpg"
			});

			source.Add(new Monkey
			{
				Name = "Squirrel Monkey",
				Location = "Central & South America",
				Details = "The squirrel monkeys are the New World monkeys of the genus Saimiri. They are the only genus in the subfamily Saimirinae. The name of the genus Saimiri is of Tupi origin, and was also used as an English name by early researchers.",
				ImageUrl = "saimiri.jpg"
			});

			source.Add(new Monkey
			{
				Name = "Golden Lion Tamarin",
				Location = "Brazil",
				Details = "The golden lion tamarin also known as the golden marmoset, is a small New World monkey of the family Callitrichidae.",
				ImageUrl = "golden.jpg"
			});

			source.Add(new Monkey
			{
				Name = "Howler Monkey",
				Location = "South America",
				Details = "Howler monkeys are among the largest of the New World monkeys. Fifteen species are currently recognised. Previously classified in the family Cebidae, they are now placed in the family Atelidae.",
				ImageUrl = "alouatta.jpg"
			});

			source.Add(new Monkey
			{
				Name = "Japanese Macaque",
				Location = "Japan",
				Details = "The Japanese macaque, is a terrestrial Old World monkey species native to Japan. They are also sometimes known as the snow monkey because they live in areas where snow covers the ground for months each",
				ImageUrl = "papio.jpg"
			});

			source.Add(new Monkey
			{
				Name = "Mandrill",
				Location = "Southern Cameroon, Gabon, Equatorial Guinea, and Congo",
				Details = "The mandrill is a primate of the Old World monkey family, closely related to the baboons and even more closely to the drill. It is found in southern Cameroon, Gabon, Equatorial Guinea, and Congo.",
				ImageUrl = "capuchin.jpg"
			});

			source.Add(new Monkey
			{
				Name = "Proboscis Monkey",
				Location = "Borneo",
				Details = "The proboscis monkey or long-nosed monkey, known as the bekantan in Malay, is a reddish-brown arboreal Old World monkey that is endemic to the south-east Asian island of Borneo.",
				ImageUrl = "bluemonkey.jpg"
			});

			source.Add(new Monkey
			{
				Name = "Red-shanked Douc",
				Location = "Vietnam, Laos",
				Details = "The red-shanked douc is a species of Old World monkey, among the most colourful of all primates.",
				ImageUrl = "saimiri.jpg"
			});

			source.Add(new Monkey
			{
				Name = "Gray-shanked Douc",
				Location = "Vietnam",
				Details = "The gray-shanked douc langur is a douc species native to the Vietnamese provinces of Quảng Nam, Quảng Ngãi, Bình Định, Kon Tum, and Gia Lai.",
				ImageUrl = "golden.jpg"
			});

			source.Add(new Monkey
			{
				Name = "Golden Snub-nosed Monkey",
				Location = "China",
				Details = "The golden snub-nosed monkey is an Old World monkey in the Colobinae subfamily. It is endemic to a small area in temperate, mountainous forests of central and Southwest China.",
				ImageUrl = "alouatta.jpg"
			});

			source.Add(new Monkey
			{
				Name = "Black Snub-nosed Monkey",
				Location = "China",
				Details = "The black snub-nosed monkey, also known as the Yunnan snub-nosed monkey, is an endangered species of primate in the family Cercopithecidae.",
				ImageUrl = "papio.jpg"
			});

			source.Add(new Monkey
			{
				Name = "Tonkin Snub-nosed Monkey",
				Location = "Vietnam",
				Details = "The Tonkin snub-nosed monkey or Dollman's snub-nosed monkey is a slender-bodied arboreal Old World monkey, endemic to northern Vietnam.",
				ImageUrl = "capuchin.jpg"
			});

			source.Add(new Monkey
			{
				Name = "Thomas's Langur",
				Location = "Indonesia",
				Details = "Thomas's langur is a species of primate in the family Cercopithecidae. It is endemic to North Sumatra, Indonesia.",
				ImageUrl = "bluemonkey.jpg"
			});

			source.Add(new Monkey
			{
				Name = "Purple-faced Langur",
				Location = "Sri Lanka",
				Details = "The purple-faced langur, also known as the purple-faced leaf monkey, is a species of Old World monkey that is endemic to Sri Lanka.",
				ImageUrl = "saimiri.jpg"
			});

			source.Add(new Monkey
			{
				Name = "Gelada",
				Location = "Ethiopia",
				Details = "The gelada, sometimes called the bleeding-heart monkey or the gelada baboon, is a species of Old World monkey found only in the Ethiopian Highlands.",
				ImageUrl = "golden.jpg"
			});

			Monkeys = new ObservableCollection<Monkey>(source);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
